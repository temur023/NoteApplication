using Clean.Application.Dtos.User;
using Clean.Application.Services;
using Clean.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace NoteApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService service):Controller
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        return Ok(await service.Login(dto));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserCreateDto dto)
    {
        return Ok(await service.Create(dto));
    }
}