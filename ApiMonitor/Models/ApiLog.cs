namespace ApiMonitor.Models;

public class ApiLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int StatusCode { get; set; }
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? InterfaceBody { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string OriginHost { get; set; } = string.Empty;
    public bool AlertSent { get; set; } = false;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
