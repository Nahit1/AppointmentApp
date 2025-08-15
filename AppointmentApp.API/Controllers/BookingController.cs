using System.Security.Claims;
using AppointmentApp.API.Context;
using AppointmentApp.API.DTOs;
using AppointmentApp.API.Entities;
using AppointmentApp.API.Enums;
using AppointmentApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentApp.API.Controllers;


[ApiController]
[Route("api/bookings")]
public class BookingController:ControllerBase
{
    private readonly IBookingService _svc;
    public BookingController(IBookingService svc) => _svc = svc;

    Guid CurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue(ClaimTypes.Name) ?? throw new UnauthorizedAccessException());

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest req, CancellationToken ct)
    {
        var id = await _svc.CreateAsync(req, CurrentUserId(), ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    // Basit get (MVP)
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get([FromServices] AppDbContext db, Guid id, CancellationToken ct)
    {
        var b = await db.Bookings.FindAsync([id], ct);
        return b is null ? NotFound() : Ok(b);
    }

    [Authorize]
    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
    {
        await _svc.AcceptAsync(id, CurrentUserId(), ct);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
    {
        await _svc.RejectAsync(id, CurrentUserId(), ct);
        return NoContent();
    }
}