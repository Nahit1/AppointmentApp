namespace AppointmentApp.API.DTOs;

public record CreateAvailabilityRuleRequest(
    DayOfWeek[] Days, string StartTime, string EndTime, int BufferMin,
    string? ValidFrom, string? ValidTo, string? Timezone
);