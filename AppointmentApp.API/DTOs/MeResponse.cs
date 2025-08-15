namespace AppointmentApp.API.DTOs;

public record MeResponse(
    Guid Id, string Email, string FullName, List<string> Roles,
    CoachProfileDto? CoachProfile
);