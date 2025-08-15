using AppointmentApp.API.DTOs;

namespace AppointmentApp.API.Interfaces;

public interface IAuthService
{
    Task<Guid> RegisterAsync(RegisterRequest req, CancellationToken ct);
    Task<string> LoginAsync(LoginRequest req, CancellationToken ct);
    Task<MeResponse> MeAsync(Guid userId, CancellationToken ct);
    Task BecomeCoachAsync(Guid currentUserId, BecomeCoachRequest req, CancellationToken ct);
}
