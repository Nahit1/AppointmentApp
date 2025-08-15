using AppointmentApp.API.DTOs;

namespace AppointmentApp.API.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetMyAsync(Guid currentUserId, int skip, int take, CancellationToken ct);
    Task MarkAsReadAsync(Guid notificationId, Guid currentUserId, CancellationToken ct);
}