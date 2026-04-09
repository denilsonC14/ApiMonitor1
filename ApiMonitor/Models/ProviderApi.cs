namespace ApiMonitor.Models;

public class ProviderApi
{
   
    public Guid Id { get; set; } = Guid.NewGuid();

    
    public string Name { get; set; } = string.Empty;

    
    
    public string Url { get; set; } = string.Empty;

  
    public string HttpMethod { get; set; } = "GET";

    
    public int IntervalSeconds { get; set; } = 60;

   
    public bool IsActive { get; set; } = true;

    public int? LastStatusCode { get; set; } = null;

    public DateTime? LastAlertSentAt { get; set; }

   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    
    // EF Core usa esto para relacionar las dos tablas
    public ICollection<ApiMonitorLog> Logs { get; set; }
        = new List<ApiMonitorLog>();
}