using ApiMonitor.Data;
using ApiMonitor.Dto;
using ApiMonitor.Models;

namespace ApiMonitor.Services;

public class ApiLogService
{
    private readonly AppDbContext _context;
    private readonly AlertService _alertService;

    public ApiLogService(AppDbContext context, AlertService alertService)
    {
        _context      = context;
        _alertService = alertService;
    }

    public async Task ProcessAsync(ApiLogDto dto)
    {
        var log = new ApiLog
        {
            StatusCode       = dto.StatusCode,
            RequestBody      = dto.RequestBody,
            ResponseBody     = dto.ResponseBody,
            Method           = dto.Method,
            Path             = dto.Path,
            InterfaceBody    = dto.InterfaceBody,
            ProviderName     = dto.ProviderName,
            Host             = dto.Host,
            OriginHost       = dto.OriginHost
        };

        if (log.StatusCode != 200)
        {
            await _alertService.SendAlertAsync(log);
            log.AlertSent = true;
        }

        _context.ApiLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
