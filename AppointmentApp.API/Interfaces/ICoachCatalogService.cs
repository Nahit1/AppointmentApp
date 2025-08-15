using AppointmentApp.API.DTOs;

namespace AppointmentApp.API.Interfaces;

public interface ICoachCatalogService
{
    Task<Guid> CreateServiceAsync(Guid coachId, CreateServiceRequest req, CancellationToken ct);
    Task<Guid> CreateAvailabilityRuleAsync(Guid coachId, CreateAvailabilityRuleRequest req, CancellationToken ct);
    Task<List<SlotResponse>> GetSlotsAsync(Guid coachId, Guid serviceId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
}