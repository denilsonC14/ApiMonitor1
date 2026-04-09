namespace ApiMonitor.Models;

public class AlertSettings
{
    public TelegramSettings Telegram { get; set; } = new();
   
}

public class TelegramSettings
{
    public bool Enabled { get; set; }
    public string BotToken { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
}

