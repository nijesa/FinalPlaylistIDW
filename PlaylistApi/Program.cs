using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlaylistApi.Common;
using PlaylistApi.Data;
using PlaylistApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Base de datos: EF Core + SQLite ----------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// ---------- Servicios por inyección de dependencias ----------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISongsService, SongsService>();

// ---------- Controllers + errores de validación con el formato de NestJS ----------
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var messages = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Solicitud inválida" : e.ErrorMessage)
                .ToArray();

            // { "statusCode": 400, "message": ["..."], "error": "Bad Request" }
            return new BadRequestObjectResult(new
            {
                statusCode = 400,
                message = messages,
                error = "Bad Request"
            });
        };
    });

// ---------- Requisito 7: CORS para Vue (5173) e Ionic (8100 + Capacitor) ----------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Clients", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()   // incluye Authorization y Content-Type
              .AllowAnyMethod()); // incluye PATCH y DELETE
});

// ---------- Requisitos 8-10: JWT ----------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
          ?? throw new InvalidOperationException("Falta la sección 'Jwt' en appsettings.json");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Mantiene los nombres originales de los claims ("sub", "email") en vez de renombrarlos.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Respuesta 401 con el mismo cuerpo que NestJS: { "message": "Unauthorized", "statusCode": 401 }
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Unauthorized", statusCode = 401 });
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Aplica las migraciones pendientes al arrancar (crea playlist.db si no existe).
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

// ---------- Pipeline (el orden importa) ----------
app.UseCors("Clients");                          // 1. CORS primero, para que responda los preflight (OPTIONS)
app.UseMiddleware<ErrorHandlingMiddleware>();    // 2. errores -> JSON con formato NestJS
app.UseAuthentication();                         // 3. ¿quién eres? (lee el JWT)
app.UseAuthorization();                          // 4. ¿puedes entrar? ([Authorize])
app.MapControllers();

app.Run();
