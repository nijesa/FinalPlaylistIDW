using Microsoft.AspNetCore.Mvc;
using PlaylistApi.Dtos;
using PlaylistApi.Services;

namespace PlaylistApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST /api/auth/register -> 201 { id, email }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = await _authService.RegisterAsync(dto);
        return StatusCode(StatusCodes.Status201Created, user);
    }

    // POST /api/auth/login -> 200 { accessToken, user: { id, email } }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }
}
