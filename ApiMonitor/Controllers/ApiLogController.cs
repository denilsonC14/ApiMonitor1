using ApiMonitor.Dto;
using ApiMonitor.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiMonitor.Controllers;

[ApiController]
[Route("api/logs")]
public class ApiLogController : ControllerBase
{
    private readonly ApiLogService _apiLogService;

    public ApiLogController(ApiLogService apiLogService)
    {
        _apiLogService = apiLogService;
    }

    [HttpPost]
    public async Task<IActionResult> Receive([FromBody] ApiLogDto dto)
    {
        await _apiLogService.ProcessAsync(dto);

        return Ok(new { message = "Log recibido" });
    }
}
