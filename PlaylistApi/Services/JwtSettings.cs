namespace PlaylistApi.Services;

// Se llena desde la sección "Jwt" de appsettings.json (equivale a JWT_SECRET / JWT_EXPIRES_IN del .env de NestJS).
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 1440; // 1 día, igual que "1d" en NestJS
}
