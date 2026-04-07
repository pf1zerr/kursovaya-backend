using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudyHub.API.Data;
using StudyHub.API.DTOs;
using StudyHub.API.Models;

namespace StudyHub.API.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return null;

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new AuthResponseDto(GenerateToken(user), user.Username, user.Role);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        return new AuthResponseDto(GenerateToken(user), user.Username, user.Role);
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        return user == null ? null : new UserProfileDto(user.Username, user.Email, user.Role, user.CreatedAt);
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return (false, "User not found");
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return (false, "Current password is incorrect");
        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            return (false, "New password must be at least 6 characters");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();
        return (true, "Password updated");
    }

    private string GenerateToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "supersecretkey_studyhub_2024_!@#$";
        var issuer = _config["Jwt:Issuer"] ?? "StudyHub";
        var audience = _config["Jwt:Audience"] ?? "StudyHubUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
