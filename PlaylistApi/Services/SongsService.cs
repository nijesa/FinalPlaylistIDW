using Microsoft.EntityFrameworkCore;
using PlaylistApi.Common;
using PlaylistApi.Data;
using PlaylistApi.Dtos;
using PlaylistApi.Models;

namespace PlaylistApi.Services;

public interface ISongsService
{
    Task<PagedSongsDto> FindAllAsync(int userId, string? page, string? limit, string? search);
    Task<SongResponseDto> FindOneAsync(int id, int userId);
    Task<SongResponseDto> CreateAsync(CreateSongDto dto, int userId);
    Task<SongResponseDto> UpdateAsync(int id, int userId, UpdateSongDto dto);
    Task<SongResponseDto> RemoveAsync(int id, int userId);
}

public class SongsService : ISongsService
{
    private readonly AppDbContext _db;

    public SongsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedSongsDto> FindAllAsync(int userId, string? page, string? limit, string? search)
    {
        var pageNumber = ParseOrDefault(page, 1);
        var pageSize = ParseOrDefault(limit, 10);

        // Solo las canciones de ESTE usuario
        var query = _db.Songs.AsNoTracking().Where(s => s.UserId == userId);

        // Búsqueda por nombre ("contiene", sin distinguir mayúsculas, como en Prisma + SQLite)
        var term = search?.Trim();
        if (!string.IsNullOrEmpty(term))
        {
            var pattern = $"%{EscapeLike(term)}%";
            query = query.Where(s => EF.Functions.Like(s.Name, pattern, "\\"));
        }

        // Ojo: el DbContext no es thread-safe, por eso estas dos consultas van una tras otra
        // (en NestJS usábamos Promise.all; aquí NO se debe usar Task.WhenAll con el mismo contexto).
        var total = await query.CountAsync();

        var skip = (int)Math.Min((long)(pageNumber - 1) * pageSize, int.MaxValue);
        var songs = await query
            .OrderByDescending(s => s.Id)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));

        return new PagedSongsDto(
            songs.Select(ToDto).ToList(),
            new PaginationMetaDto(total, pageNumber, pageSize, totalPages));
    }

    public async Task<SongResponseDto> FindOneAsync(int id, int userId)
    {
        var song = await FindOwnedAsync(id, userId);
        return ToDto(song);
    }

    public async Task<SongResponseDto> CreateAsync(CreateSongDto dto, int userId)
    {
        var now = DateTime.UtcNow;
        var song = new Song
        {
            Name = dto.Name,
            Singer = dto.Singer,
            UserId = userId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Songs.Add(song);
        await _db.SaveChangesAsync();
        return ToDto(song);
    }

    public async Task<SongResponseDto> UpdateAsync(int id, int userId, UpdateSongDto dto)
    {
        var song = await FindOwnedAsync(id, userId); // valida que exista Y que sea del usuario

        // PATCH parcial: solo se cambia lo que venga en el body
        if (dto.Name is not null) song.Name = dto.Name;
        if (dto.Singer is not null) song.Singer = dto.Singer;
        song.UpdatedAt = DateTime.UtcNow; // equivale a @updatedAt de Prisma

        await _db.SaveChangesAsync();
        return ToDto(song);
    }

    public async Task<SongResponseDto> RemoveAsync(int id, int userId)
    {
        var song = await FindOwnedAsync(id, userId);
        var deleted = ToDto(song); // NestJS devuelve la canción eliminada, hacemos lo mismo

        _db.Songs.Remove(song);
        await _db.SaveChangesAsync();
        return deleted;
    }

    // ---------- helpers ----------

    // Si la canción no existe O es de otro usuario -> el mismo 404,
    // para no revelar que existe una canción ajena con ese id.
    private async Task<Song> FindOwnedAsync(int id, int userId)
    {
        var song = await _db.Songs.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        if (song is null)
            throw new HttpException(404, $"Canción {id} no encontrada");
        return song;
    }

    // Replica Math.max(1, Number(x) || fallback) de NestJS:
    // texto inválido o 0 -> valor por defecto; negativos -> 1.
    private static int ParseOrDefault(string? raw, int fallback)
    {
        if (!int.TryParse(raw, out var n) || n == 0) return fallback;
        return Math.Max(1, n);
    }

    // Para que % y _ escritos por el usuario se busquen como texto literal
    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    private static SongResponseDto ToDto(Song s) =>
        new(s.Id, s.Name, s.Singer, s.CreatedAt, s.UpdatedAt, s.UserId);
}
