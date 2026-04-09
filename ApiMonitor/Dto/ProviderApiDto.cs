namespace ApiMonitor.Dto;

// DTO para CREAR un proveedor — solo los campos que el usuario envía
public class CreateProviderApiDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "GET";
    public int IntervalSeconds { get; set; } = 60;
}

// DTO para EDITAR un proveedor
public class UpdateProviderApiDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "GET";
    public int IntervalSeconds { get; set; } = 60;
    public bool IsActive { get; set; } = true;
}

// DTO de RESPUESTA — lo que devuelve la API al cliente
public class ProviderApiResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public int IntervalSeconds { get; set; }
    public bool IsActive { get; set; }
    public int LastStatusCode { get; set; }
    public DateTime CreatedAt { get; set; }
}