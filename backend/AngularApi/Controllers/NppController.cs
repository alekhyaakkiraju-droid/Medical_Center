using System.Security.Claims;
using AngularApi.Contracts.Services.Interfaces;
using AngularApi.Contracts.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NppController : ControllerBase
{
    private readonly INppService _nppService;

    public NppController(INppService nppService)
    {
        _nppService = nppService;
    }

    [HttpGet("status")]
    public async Task<ActionResult<NppStatusResponse>> GetStatus(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        return Ok(await _nppService.GetStatusAsync(userId, cancellationToken));
    }

    [HttpPost("acknowledge")]
    public async Task<IActionResult> Acknowledge(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        await _nppService.AcknowledgeAsync(userId, cancellationToken);
        return Ok(new { message = "NPP acknowledgment recorded" });
    }

    [HttpGet("content")]
    public async Task<ActionResult<NppContentResponse>> GetContent(CancellationToken cancellationToken)
    {
        return Ok(await _nppService.GetContentAsync(cancellationToken));
    }
}
