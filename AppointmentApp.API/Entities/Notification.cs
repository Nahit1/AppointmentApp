namespace AppointmentApp.API.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RecipientUserId { get; set; }
    public User RecipientUser { get; set; } = default!;

    public string Message { get; set; } = default!;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}