namespace AppointmentApp.API.DTOs;

public record CoachProfileDto(
    string Status, string Bio, string City, int BasePriceCents, string PriceCurrency, List<string> Specialties,
    int ExperienceYears, string? Instagram, string? Phone
);