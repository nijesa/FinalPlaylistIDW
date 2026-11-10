using System.ComponentModel.DataAnnotations;

namespace PlaylistApi.Dtos;

// ---------- Peticiones ----------

public class CreateSongDto
{
    [Required(ErrorMessage = "El nombre de la canción es obligatorio")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cantante es obligatorio")]
    [MaxLength(150)]
    public string Singer { get; set; } = string.Empty;
}

// PATCH: todos los campos son opcionales (equivale a PartialType de NestJS).
// Si llegan, se validan igual que al crear.
public class UpdateSongDto
{
    [MinLength(1, ErrorMessage = "El nombre de la canción es obligatorio")]
    [MaxLength(150)]
    public string? Name { get; set; }

    [MinLength(1, ErrorMessage = "El cantante es obligatorio")]
    [MaxLength(150)]
    public string? Singer { get; set; }
}

// ---------- Respuestas ----------

public record SongResponseDto(
    int Id,
    string Name,
    string Singer,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int UserId);

public record PaginationMetaDto(int Total, int Page, int Limit, int TotalPages);

// { "data": [...], "meta": { total, page, limit, totalPages } }  <- misma forma que NestJS
public record PagedSongsDto(IReadOnlyList<SongResponseDto> Data, PaginationMetaDto Meta);
