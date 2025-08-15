using AppointmentApp.API.Context;
using AppointmentApp.API.DTOs;
using AppointmentApp.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppointmentApp.API.Services;

public class NotificationService(AppDbContext db) : INotificationService
{
    public async Task<List<NotificationDto>> GetMyAsync(Guid currentUserId, int skip, int take, CancellationToken ct)
    {
        take = Math.Clamp(take, 1, 100);
        return await db.Notifications
            .Where(n => n.RecipientUserId == currentUserId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip).Take(take)
            .Select(n => new NotificationDto(n.Id, n.Message, n.IsRead, n.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid currentUserId, CancellationToken ct)
    {
        var n = await db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.RecipientUserId == currentUserId, ct)
                ?? throw new KeyNotFoundException("Notification not found");
        if (!n.IsRead) { n.IsRead = true; await db.SaveChangesAsync(ct); }
    }
}