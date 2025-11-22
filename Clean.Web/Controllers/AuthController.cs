using Clean.Application.Abstractions;
using Clean.Application.Dtos.User;
using Clean.Application.Services;
using Clean.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace NoteApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService service):ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        return Ok(await service.Login(dto));
    }
    
    [PermissionAuthorize(PermissionConstants.Users.Manage)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserCreateDto dto)
    {
        return Ok(await service.Create(dto));
    }
}