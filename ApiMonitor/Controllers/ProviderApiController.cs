using ApiMonitor.Data;
using ApiMonitor.Dto;
using ApiMonitor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiMonitor.Controllers;

[ApiController]
[Route("api/providers")]
public class ProviderApiController : ControllerBase
{
    private readonly AppDbContext _context;

  
    public ProviderApiController(AppDbContext context)
    {
        _context = context;
    }

 
    [HttpGet]
    public async Task<ActionResult<List<ProviderApiResponseDto>>> GetAll()
    {
        var providers = await _context.ProviderApis
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProviderApiResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Url = p.Url,
                HttpMethod = p.HttpMethod,
                IntervalSeconds = p.IntervalSeconds,
                IsActive = p.IsActive,
                LastStatusCode = p.LastStatusCode ?? 0,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return Ok(providers);
    }

    // ─────────────────────────────────────────
    // GET /api/providers/{id}
    // Devuelve un proveedor por su Id
    // ─────────────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProviderApiResponseDto>> GetById(Guid id)
    {
        var provider = await _context.ProviderApis.FindAsync(id);

        if (provider is null)
            return NotFound(new { message = "Proveedor no encontrado" });

        return Ok(new ProviderApiResponseDto
        {
            Id = provider.Id,
            Name = provider.Name,
            Url = provider.Url,
            HttpMethod = provider.HttpMethod,
            IntervalSeconds = provider.IntervalSeconds,
            IsActive = provider.IsActive,
            LastStatusCode = provider.LastStatusCode ?? 0,
            CreatedAt = provider.CreatedAt
        });
    }

    // ─────────────────────────────────────────
    // POST /api/providers
    // Registra una nueva URL a monitorear
    // ─────────────────────────────────────────
    [HttpPost]
    public async Task<ActionResult<ProviderApiResponseDto>> Create(
        [FromBody] CreateProviderApiDto dto)
    {
        var provider = new ProviderApi
        {
            Name = dto.Name,
            Url = dto.Url,
            HttpMethod = dto.HttpMethod,
            IntervalSeconds = dto.IntervalSeconds
        };

        _context.ProviderApis.Add(provider);
        await _context.SaveChangesAsync();

        // 201 Created + la URL donde se puede consultar el recurso
        return CreatedAtAction(
            nameof(GetById),
            new { id = provider.Id },
            new ProviderApiResponseDto
            {
                Id = provider.Id,
                Name = provider.Name,
                Url = provider.Url,
                HttpMethod = provider.HttpMethod,
                IntervalSeconds = provider.IntervalSeconds,
                IsActive = provider.IsActive,
                LastStatusCode = provider.LastStatusCode ?? 0,
                CreatedAt = provider.CreatedAt
            });
    }

    // ─────────────────────────────────────────
    // PUT /api/providers/{id}
    // Edita un proveedor existente
    // ─────────────────────────────────────────
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateProviderApiDto dto)
    {
        var provider = await _context.ProviderApis.FindAsync(id);

        if (provider is null)
            return NotFound(new { message = "Proveedor no encontrado" });

        provider.Name = dto.Name;
        provider.Url = dto.Url;
        provider.HttpMethod = dto.HttpMethod;
        provider.IntervalSeconds = dto.IntervalSeconds;
        provider.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ─────────────────────────────────────────
    // DELETE /api/providers/{id}
    // Elimina un proveedor y sus logs (Cascade)
    // ─────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var provider = await _context.ProviderApis.FindAsync(id);

        if (provider is null)
            return NotFound(new { message = "Proveedor no encontrado" });

        _context.ProviderApis.Remove(provider);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ─────────────────────────────────────────
    // GET /api/providers/{id}/logs
    // Devuelve los últimos 50 logs de un proveedor
    // ─────────────────────────────────────────
    [HttpGet("{id:guid}/logs")]
    public async Task<IActionResult> GetLogs(Guid id)
    {
        var exists = await _context.ProviderApis.AnyAsync(p => p.Id == id);

        if (!exists)
            return NotFound(new { message = "Proveedor no encontrado" });

        var logs = await _context.ApiMonitorLogs
            .Where(l => l.ProviderApiId == id)
            .OrderByDescending(l => l.CheckedAt)
            .Take(50)
            .Select(l => new
            {
                l.Id,
                l.StatusCode,
                l.ResponseJson,
                l.ErrorMessage,
                l.ResponseTimeMs,
                l.CheckedAt,
                l.AlertSent
            })
            .ToListAsync();

        return Ok(logs);
    }
}