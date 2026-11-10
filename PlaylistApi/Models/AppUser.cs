namespace PlaylistApi.Models;

// Equivale al model "User" de Prisma.
public class AppUser
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;

    // Aquí se guarda el HASH de bcrypt, nunca la contraseña en texto plano.
    public string Password { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Song> Songs { get; set; } = new();
}
