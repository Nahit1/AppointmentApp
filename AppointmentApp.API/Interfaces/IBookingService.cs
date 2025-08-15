using AppointmentApp.API.DTOs;

namespace AppointmentApp.API.Interfaces;

public interface IBookingService
{
    Task<Guid> CreateAsync(CreateBookingRequest req, Guid currentUserId, CancellationToken ct);
    Task AcceptAsync(Guid bookingId, Guid currentUserId, CancellationToken ct);
    Task RejectAsync(Guid bookingId, Guid currentUserId, CancellationToken ct);
}