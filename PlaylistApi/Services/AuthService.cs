using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PlaylistApi.Common;
using PlaylistApi.Data;
using PlaylistApi.Dtos;
using PlaylistApi.Models;

namespace PlaylistApi.Services;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(RegisterDto dto);
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtSettings _jwt;

    public AuthService(AppDbContext db, IOptions<JwtSettings> jwtOptions)
    {
        _db = db;
        _jwt = jwtOptions.Value;
    }

    // Requisito 8: registro guardando la contraseña hasheada
    public async Task<UserResponseDto> RegisterAsync(RegisterDto dto)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists)
            throw new HttpException(409, "Ya existe un usuario con ese correo");

        var user = new AppUser
        {
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 10)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Nunca se devuelve la contraseña, ni siquiera hasheada.
        return new UserResponseDto(user.Id, user.Email);
    }

    // Requisito 9: login que emite un JWT firmado
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        // Mismo mensaje si el correo no existe o si la contraseña no coincide,
        // para no revelar cuál de los dos falló.
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            throw new HttpException(401, "Credenciales inválidas");

        return new LoginResponseDto(CreateToken(user), new UserResponseDto(user.Id, user.Email));
    }

    private string CreateToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            // Mismo payload que en NestJS: { sub: user.id, email: user.email }
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            }),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiresInMinutes),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
