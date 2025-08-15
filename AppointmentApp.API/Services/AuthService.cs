using AppointmentApp.API.Context;
using AppointmentApp.API.DTOs;
using AppointmentApp.API.Entities;
using AppointmentApp.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppointmentApp.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;

    public AuthService(AppDbContext db, IJwtService jwt)
    {
        _db = db; _jwt = jwt;
    }

    public async Task<Guid> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw new InvalidOperationException("Email already exists");

        var user = new User {
            Email = email,
            FullName = req.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        _db.Users.Add(user);
        _db.UserRoles.Add(new UserRole { User = user, RoleId = 1 }); // student
        await _db.SaveChangesAsync(ct);
        return user.Id;
    }

    public async Task<string> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct)
                   ?? throw new UnauthorizedAccessException("Invalid credentials");
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        return await _jwt.GenerateAsync(user.Id, user.Email);
    }

    public async Task<MeResponse> MeAsync(Guid userId, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == userId, ct);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        var profile = await _db.CoachProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        CoachProfileDto? coachDto = profile is null ? null : new CoachProfileDto(
            profile.Status, profile.Bio, profile.City, profile.BasePriceCents, profile.PriceCurrency,
            profile.Specialties.ToList(), profile.ExperienceYears, profile.Instagram, profile.Phone
        );

        return new MeResponse(user.Id, user.Email, user.FullName, roles, coachDto);
    }

    public async Task BecomeCoachAsync(Guid currentUserId, BecomeCoachRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, ct)
                   ?? throw new KeyNotFoundException("User not found");

        // coach rolü ekle (varsa ekleme)
        bool hasCoach = await _db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == 2, ct);
        if (!hasCoach)
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = 2 });

        var profile = await _db.CoachProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id, ct);
        if (profile is null)
        {
            profile = new CoachProfile {
                UserId = user.Id,
                Bio = req.Bio,
                Specialties = req.Specialties.ToArray(),
                ExperienceYears = req.ExperienceYears,
                City = req.City,
                BasePriceCents = req.BasePriceCents,
                PriceCurrency = req.PriceCurrency,
                Instagram = req.Instagram,
                Phone = req.Phone,
                Status = "pending"
            };
            _db.CoachProfiles.Add(profile);
        }
        else
        {
            profile.Bio = req.Bio;
            profile.Specialties = req.Specialties.ToArray();
            profile.ExperienceYears = req.ExperienceYears;
            profile.City = req.City;
            profile.BasePriceCents = req.BasePriceCents;
            profile.PriceCurrency = req.PriceCurrency;
            profile.Instagram = req.Instagram;
            profile.Phone = req.Phone;
            // profile.Status = "pending"; // moderasyon istiyorsan tekrar çağa
        }

        await _db.SaveChangesAsync(ct);
    }
}