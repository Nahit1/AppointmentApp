using AppointmentApp.API.Enums;

namespace AppointmentApp.API.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CoachId { get; set; }
    public User Coach { get; set; } = default!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = default!;

    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}