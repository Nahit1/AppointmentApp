namespace AppointmentApp.API.DTOs;

public record SlotResponse(DateTimeOffset StartsAt, DateTimeOffset EndsAt);