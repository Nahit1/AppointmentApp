namespace AppointmentApp.API.Entities;

public class Service
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CoachId { get; set; }
    public User Coach { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int DurationMin { get; set; }           // 60
    public int BasePriceCents { get; set; }        // 350000
    public string Currency { get; set; } = "TRY";
    public bool IsActive { get; set; } = true;
}