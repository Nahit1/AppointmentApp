namespace AppointmentApp.API.DTOs;

public record RegisterRequest(string Email, string FullName, string Password);