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
        _settings = settings.Value;
        _logger = logger;
    }

    // ─────────────────────────────────────────
    // Alerta de CAÍDA
    // ─────────────────────────────────────────
    public async Task SendDownAlertAsync(
       ProviderApi provider, CheckResult result, DateTime checkTime)
    {
        var telegramMsg =
            $"🔴 <b>ALERTA DE CAÍDA</b>\n\n" +
            $"🏷️ <b>Proveedor:</b> {provider.Name}\n" +
            $"🌐 <b>URL:</b> {provider.Url}\n" +
            $"📊 <b>Status:</b> {(result.StatusCode == 0 ? "Sin respuesta" : result.StatusCode.ToString())}\n" +
          //  $"⏱️ <b>Tiempo:</b> {result.ResponseTimeMs} ms\n" +
            $"🕐 <b>Fecha/Hora:</b> {checkTime:yyyy-MM-dd HH:mm:ss} UTC"; // ← misma fecha

        await SendTelegramAsync(telegramMsg);
      
    }

    public async Task SendRecoveryAlertAsync(ProviderApi provider, DateTime checkTime)
    {
        var telegramMsg =
            $"✅ <b>SERVICIO RECUPERADO</b>\n\n" +
            $"🏷️ <b>Proveedor:</b> {provider.Name}\n" +
            $"🌐 <b>URL:</b> {provider.Url}\n" +
            $"🕐 <b>Recuperado:</b> {checkTime:yyyy-MM-dd HH:mm:ss} UTC"; // ← misma fecha

        await SendTelegramAsync(telegramMsg);
        
    }


    // ─────────────────────────────────────────
    // Envío a Telegram
    // ─────────────────────────────────────────
    private async Task SendTelegramAsync(string message)
    {
        if (!_settings.Telegram.Enabled) return;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.telegram.org/bot{_settings.Telegram.BotToken}/sendMessage";

            var payload = new
            {
                chat_id = _settings.Telegram.ChatId,
                text = message,
                parse_mode = "HTML"  // permite <b>, <code>, etc.
            };

            var json = JsonSerializer.Serialize(payload);
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
            // Si Telegram falla no rompemos el monitor
            _logger.LogError(ex, "Error enviando alerta a Telegram");
        }
    }

   
    private string Truncate(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "Sin respuesta";
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }
}