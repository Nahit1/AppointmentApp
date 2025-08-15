namespace AppointmentApp.API.Interfaces;

public interface IJwtService { Task<string> GenerateAsync(Guid userId, string email); }