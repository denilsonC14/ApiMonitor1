namespace ApiMonitor.Models;

public class ApiMonitorLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    
    public Guid ProviderApiId { get; set; }

   
    public int StatusCode { get; set; }

    public string? ResponseJson { get; set; }

  
    public string? ErrorMessage { get; set; }

    // Cuántos milisegundos tardó en responder
    public long ResponseTimeMs { get; set; }

   
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

   
    public bool AlertSent { get; set; } = false;

    // Navegación inversa — EF Core relaciona con ProviderApi
    public ProviderApi ProviderApi { get; set; } = null!;
}