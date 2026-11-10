using System.ComponentModel.DataAnnotations;

namespace PlaylistApi.Dtos;

// ---------- Peticiones ----------

public class RegisterDto
{
    [Required(ErrorMessage = "El correo no es válido")]
    [EmailAddress(ErrorMessage = "El correo no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required(ErrorMessage = "El correo no es válido")]
    [EmailAddress(ErrorMessage = "El correo no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "password should not be empty")]
    public string Password { get; set; } = string.Empty;
}

// ---------- Respuestas ----------
// Los nombres de las propiedades salen en camelCase automáticamente (id, email, accessToken, user).

public record UserResponseDto(int Id, string Email);

public record LoginResponseDto(string AccessToken, UserResponseDto User);
