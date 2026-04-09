using ApiMonitor.Data;
using ApiMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMonitor.Services;

public class MonitorBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonitorBackgroundService> _logger;
    private readonly ApiCheckerService _checker;

    
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, DateTime>
        _lastCheckTimes = new();

    public MonitorBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<MonitorBackgroundService> logger,
        ApiCheckerService checker)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _checker = checker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Monitor iniciado");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunChecksAsync(stoppingToken);
        }
    }

    private async Task RunChecksAsync(CancellationToken stoppingToken)
    {
        // Scope solo para leer los proveedores — se cierra enseguida
        List<ProviderApi> providers;
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            providers = await context.ProviderApis
                .Where(p => p.IsActive)
                .ToListAsync(stoppingToken);
        }

        if (!providers.Any()) return;

        _logger.LogInformation("Revisando {Count} proveedores...", providers.Count);

        var semaphore = new SemaphoreSlim(10);

        var tasks = providers.Select(async provider =>
        {
            await semaphore.WaitAsync(stoppingToken);
            try
            {
                // Cada proveedor tiene su PROPIO scope — su propio DbContext
                // Así nunca hay dos operaciones en el mismo contexto
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();
                var alertService = scope.ServiceProvider
                    .GetRequiredService<AlertService>();

                await ProcessProviderAsync(
                    provider, context, alertService, stoppingToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task ProcessProviderAsync(
        ProviderApi provider,
        AppDbContext context,
        AlertService alertService,
        CancellationToken stoppingToken)
    {
        // Control de intervalo en memoria — sin tocar la BD
        if (!ShouldCheck(provider)) return;

        // Registrar hora del check ANTES de ejecutarlo para respetar el intervalo
        _lastCheckTimes[provider.Id] = DateTime.UtcNow;

        _logger.LogInformation("Chequeando: {Name} → {Url}",
            provider.Name, provider.Url);

        // Registramos el momento del check ANTES de hacerlo
         var checkTime = DateTime.UtcNow;

        var result = await _checker.CheckAsync(provider.Url, provider.HttpMethod);

        // Refrescamos el proveedor desde ESTE scope para tener datos actuales
        var freshProvider = await context.ProviderApis.FindAsync(provider.Id);
        if (freshProvider is null) return;

        var previousStatus = freshProvider.LastStatusCode; // null = nunca chequeado
        var currentStatus = result.StatusCode;
        var isDown = currentStatus != 200;
        // null (nunca chequeado) se trata como UP para alertar en el primer fallo
        var wasDown = previousStatus.HasValue && previousStatus.Value != 200;
        var isNewFailure = isDown && !wasDown;
        var isRecovery = !isDown && wasDown;

        var log = new ApiMonitorLog
        {
            ProviderApiId = freshProvider.Id,
            StatusCode = currentStatus,
            ResponseJson = result.ResponseJson,
            ErrorMessage = result.ErrorMessage,
            ResponseTimeMs = result.ResponseTimeMs,
            CheckedAt = checkTime
        };

        if (isNewFailure)
        {
            _logger.LogWarning("CAÍDA: {Name} → Status {Status}",
                freshProvider.Name, currentStatus);

            await alertService.SendDownAlertAsync(freshProvider, result, checkTime);
            log.AlertSent = true;
            freshProvider.LastAlertSentAt = checkTime;
        }
        else if (isRecovery)
        {
            _logger.LogInformation("RECUPERADO: {Name} volvió a 200",
                freshProvider.Name);

            await alertService.SendRecoveryAlertAsync(freshProvider, checkTime);
            log.AlertSent = true;
            freshProvider.LastAlertSentAt = checkTime;
        }

        freshProvider.LastStatusCode = currentStatus;
        context.ApiMonitorLogs.Add(log);

        // Solo este scope toca este context — sin conflicto
        await context.SaveChangesAsync(stoppingToken);
    }

    // Sin consultas a BD — solo revisa el diccionario en memoria
    private bool ShouldCheck(ProviderApi provider)
    {
        if (!_lastCheckTimes.TryGetValue(provider.Id, out var lastCheck))
            return true; // nunca fue chequeado → chequear

        var elapsed = DateTime.UtcNow - lastCheck;
        return elapsed.TotalSeconds >= provider.IntervalSeconds;
    }
}