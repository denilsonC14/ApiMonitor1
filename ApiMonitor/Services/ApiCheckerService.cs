namespace ApiMonitor.Services;

// Resultado de cada check que hacemos a una API externa
public class CheckResult
{

    public int StatusCode { get; set; }      // 200, 500, 0 (sin conexión)
    public string? ResponseJson { get; set; } // el body que devolvió
    public string? ErrorMessage { get; set; } // si hubo excepción
    public long ResponseTimeMs { get; set; }  // cuánto tardó
}

public class ApiCheckerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiCheckerService> _logger;

    public ApiCheckerService(
        IHttpClientFactory httpClientFactory,
        ILogger<ApiCheckerService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<CheckResult> CheckAsync(string url, string httpMethod)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var client = _httpClientFactory.CreateClient("monitor");

        try
        {
            var method = new HttpMethod(httpMethod);
            var request = new HttpRequestMessage(method, url);
            var response = await client.SendAsync(request);

            stopwatch.Stop();

            // Leemos el body sin importar qué contenga
            var body = await response.Content.ReadAsStringAsync();

            // Si el body es muy largo lo recortamos para no llenar la BD
            if (body.Length > 2000)
                body = body[..2000] + "... [truncado]";

            return new CheckResult
            {
                StatusCode = (int)response.StatusCode,
                ResponseJson = body,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (TaskCanceledException)
        {
            // Timeout — el servidor tardó demasiado
            stopwatch.Stop();
            _logger.LogWarning("Timeout al conectar con {Url}", url);
            return new CheckResult
            {
                StatusCode = 0,
                ErrorMessage = "Timeout: la API no respondió a tiempo",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (HttpRequestException ex)
        {
            // Sin conexión, DNS fallido, puerto cerrado
            stopwatch.Stop();
            _logger.LogWarning("Error de red con {Url}: {Error}", url, ex.Message);
            return new CheckResult
            {
                StatusCode = 0,
                ErrorMessage = $"Error de red: {ex.Message}",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error inesperado al chequear {Url}", url);
            return new CheckResult
            {
                StatusCode = 0,
                ErrorMessage = $"Error inesperado: {ex.Message}",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}