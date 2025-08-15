namespace AppointmentApp.API.DTOs;

public class CreateBookingRequest
{
    public Guid CoachId { get; set; }
    public Guid StudentId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public string StudentName { get; set; } = default!;
}