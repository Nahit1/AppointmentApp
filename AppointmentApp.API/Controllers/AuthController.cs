using System.Security.Claims;
using AppointmentApp.API.DTOs;
using AppointmentApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentApp.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _svc;
    public AuthController(IAuthService svc) => _svc = svc;

    [HttpPost("register")]
    public async Task<ActionResult<Guid>> Register([FromBody] RegisterRequest req, CancellationToken ct)
        => Ok(await _svc.RegisterAsync(req, ct));

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest req, CancellationToken ct)
        => Ok(new { token = await _svc.LoginAsync(req, ct) });

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) 
                                ?? User.FindFirstValue(ClaimTypes.Name)  // fallback
                                ?? throw new UnauthorizedAccessException());
        return Ok(await _svc.MeAsync(userId, ct));
    }

    [Authorize]
    [HttpPost("become-coach")]
    public async Task<IActionResult> BecomeCoach([FromBody] BecomeCoachRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) 
                                ?? User.FindFirstValue(ClaimTypes.Name) 
                                ?? throw new UnauthorizedAccessException());
        await _svc.BecomeCoachAsync(userId, req, ct);
        return NoContent();
    }
}