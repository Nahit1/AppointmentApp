using System.Security.Claims;
using AppointmentApp.API.DTOs;
using AppointmentApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentApp.API.Controllers;

[ApiController]
[Route("api/coach")]
public class CoachCatalogController(ICoachCatalogService svc) : ControllerBase
{
    Guid CurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue(ClaimTypes.Name)
                   ?? throw new UnauthorizedAccessException());

    [Authorize(Roles = "coach,admin")]
    [HttpPost("services")]
    public async Task<ActionResult<Guid>> CreateService([FromBody] CreateServiceRequest req, CancellationToken ct)
        => Ok(await svc.CreateServiceAsync(CurrentUserId(), req, ct));

    [Authorize(Roles = "coach,admin")]
    [HttpPost("availability")]
    public async Task<ActionResult<Guid>> CreateAvailability([FromBody] CreateAvailabilityRuleRequest req, CancellationToken ct)
        => Ok(await svc.CreateAvailabilityRuleAsync(CurrentUserId(), req, ct));

    [AllowAnonymous] // öğrenci de görebilsin
    [HttpGet("slots")]
    public async Task<ActionResult<List<SlotResponse>>> GetSlots(
        [FromQuery] Guid coachId, [FromQuery] Guid serviceId,
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        CancellationToken ct)
        => Ok(await svc.GetSlotsAsync(coachId, serviceId, from, to, ct));
}