using System.ComponentModel.DataAnnotations;

namespace ApiMonitor.Dto;

public class ApiLogDto
{
    [Required]
    [Range(100, 599, ErrorMessage = "StatusCode debe ser un codigo valido")]
    public int StatusCode { get; set; }

    [Required]
    [MaxLength(10)]
    public string Method { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Path { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ProviderName { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Host { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string OriginHost { get; set; } = string.Empty;

    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public string? InterfaceBody { get; set; }
}
