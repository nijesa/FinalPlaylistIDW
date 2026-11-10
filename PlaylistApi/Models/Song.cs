namespace PlaylistApi.Models;

// Equivale al model "Song" de Prisma: id, name, singer + dueño (userId).
public class Song
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Singer { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Cada canción pertenece a un usuario.
    public int UserId { get; set; }
    public AppUser? User { get; set; }
}
