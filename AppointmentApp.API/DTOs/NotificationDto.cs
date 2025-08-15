namespace AppointmentApp.API.DTOs;

public record NotificationDto(
    Guid Id,
    string Message,
    bool IsRead,
    DateTime CreatedAt
);