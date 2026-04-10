using ApiMonitor.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace ApiMonitor.Services;

public class AlertService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AlertSettings _settings;
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        IHttpClientFactory httpClientFactory,
        IOptions<AlertSettings> settings,
        ILogger<AlertService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings          = settings.Value;
        _logger            = logger;
    }

    public async Task SendAlertAsync(ApiLog log)
    {
        var mensaje =
            $"🔴 <b>ALERTA DE FALLO</b>\n\n" +
            $"🏷️ <b>Proveedor:</b> {log.ProviderName}\n" +
            $"🌐 <b>Host:</b> {log.Host}\n" +
            $"📍 <b>Path:</b> {log.Method} {log.Path}\n" +
            $"📊 <b>Status:</b> {log.StatusCode}\n" +
            $"🕐 <b>Fecha/Hora:</b> {log.ReceivedAt:yyyy-MM-dd HH:mm:ss} UTC";

        await SendTelegramAsync(mensaje);
    }

    private async Task SendTelegramAsync(string message)
    {
        if (!_settings.Telegram.Enabled) return;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var url    = $"https://api.telegram.org/bot{_settings.Telegram.BotToken}/sendMessage";

            var payload = new
            {
                chat_id    = _settings.Telegram.ChatId,
                text       = message,
                parse_mode = "HTML"
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Telegram error: {Error}", error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando alerta a Telegram");
        }
    }
}