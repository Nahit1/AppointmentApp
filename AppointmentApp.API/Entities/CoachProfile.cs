namespace AppointmentApp.API.Entities;

public class CoachProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string Bio { get; set; } = "";
    public string[] Specialties { get; set; } = Array.Empty<string>(); // PostgreSQL text[]
    public int ExperienceYears { get; set; }
    public string City { get; set; } = "";
    public string PriceCurrency { get; set; } = "TRY";
    public int BasePriceCents { get; set; }
    public string? Instagram { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = "pending"; // pending|approved|rejected
}