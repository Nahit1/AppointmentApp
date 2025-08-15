using System.Security.Claims;
using AppointmentApp.API.DTOs;
using AppointmentApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentApp.API.Controllers;


[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(INotificationService svc) : ControllerBase
{
    Guid CurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue(ClaimTypes.Name)
                   ?? throw new UnauthorizedAccessException());

    // GET /api/notifications?skip=0&take=20
    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetMy([FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
        => Ok(await svc.GetMyAsync(CurrentUserId(), skip, take, ct));

    // POST /api/notifications/{id}/read
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        await svc.MarkAsReadAsync(id, CurrentUserId(), ct);
        return NoContent();
    }
}