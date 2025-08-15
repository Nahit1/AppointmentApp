namespace AppointmentApp.API.Entities;

public class AvailabilityRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CoachId { get; set; }
    public User Coach { get; set; } = default!;

    // Haftalık pattern (RRULE yerine basit MVP alanları)
    public DayOfWeek[] Days { get; set; } = Array.Empty<DayOfWeek>(); // örn: [Mon,Wed,Fri]
    public TimeOnly StartTime { get; set; }   // 10:00
    public TimeOnly EndTime { get; set; }     // 18:00
    public int BufferMin { get; set; } = 10;  // slot arası tampon
    public DateOnly? ValidFrom { get; set; }  // opsiyonel aralık
    public DateOnly? ValidTo { get; set; }

    public string Timezone { get; set; } = "Europe/Istanbul";
}