namespace AppointmentApp.API.DTOs;

public record BecomeCoachRequest(
    string Bio,
    List<string> Specialties,
    int ExperienceYears,
    string City,
    int BasePriceCents,
    string PriceCurrency,
    string? Instagram,
    string? Phone
);