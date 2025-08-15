using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AppointmentApp.API.Context;
using AppointmentApp.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AppointmentApp.API.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _cfg;
    private readonly AppDbContext _db;
    public JwtService(IConfiguration cfg, AppDbContext db) { _cfg = cfg; _db = db; }

    public async Task<string> GenerateAsync(Guid userId, string email)
    {
        var roles = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.Email, email)
        };
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}