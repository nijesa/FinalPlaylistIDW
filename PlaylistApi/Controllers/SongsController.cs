using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlaylistApi.Common;
using PlaylistApi.Dtos;
using PlaylistApi.Services;

namespace PlaylistApi.Controllers;

// Requisito 10: [Authorize] protege los endpoints. Se pone a nivel de clase porque, igual que en la
// versión actual de NestJS, TODAS las rutas de canciones requieren sesión (cada usuario ve solo lo suyo).
[ApiController]
[Authorize]
[Route("api/songs")]
public class SongsController : ControllerBase
{
    private readonly ISongsService _songsService;

    public SongsController(ISongsService songsService)
    {
        _songsService = songsService;
    }

    // GET /api/songs?page=1&limit=10&search=texto -> 200 { data: [...], meta: {...} }
    [HttpGet]
    public async Task<IActionResult> FindAll(
        [FromQuery] string? page,
        [FromQuery] string? limit,
        [FromQuery] string? search)
    {
        var result = await _songsService.FindAllAsync(User.GetUserId(), page, limit, search);
        return Ok(result);
    }

    // GET /api/songs/5 -> 200 canción | 404
    [HttpGet("{id:int}")]
    public async Task<IActionResult> FindOne(int id)
    {
        return Ok(await _songsService.FindOneAsync(id, User.GetUserId()));
    }

    // POST /api/songs -> 201 canción creada
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSongDto dto)
    {
        var song = await _songsService.CreateAsync(dto, User.GetUserId());
        return StatusCode(StatusCodes.Status201Created, song);
    }

    // PATCH /api/songs/5 -> 200 canción actualizada | 404
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSongDto dto)
    {
        return Ok(await _songsService.UpdateAsync(id, User.GetUserId(), dto));
    }

    // DELETE /api/songs/5 -> 200 canción eliminada | 404
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id)
    {
        return Ok(await _songsService.RemoveAsync(id, User.GetUserId()));
    }
}
