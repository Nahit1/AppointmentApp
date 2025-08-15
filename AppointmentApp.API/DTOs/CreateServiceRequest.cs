namespace AppointmentApp.API.DTOs;

public record CreateServiceRequest(string Name, int DurationMin, int BasePriceCents, string Currency);
