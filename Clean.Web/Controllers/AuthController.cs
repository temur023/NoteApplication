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
    
    [HttpPost("login/telegram")]
    public async Task<IActionResult> LoginByTelegram([FromBody] TelegramLoginRequestDto dto)
    {
        return Ok(await service.LoginByTelegram(dto));
    }
    
    [HttpPost("link-telegram")]
    public async Task<IActionResult> LinkTelegram([FromBody] LinkTelegramDto dto)
    {
        // Link Telegram account to user account (public endpoint for bot)
        var userService = HttpContext.RequestServices.GetRequiredService<IUserService>();
        await userService.SaveTelegramChatId(dto.Username, dto.ChatId);
        return Ok(new { message = "Telegram account linked successfully" });
    }
    
    [PermissionAuthorize(PermissionConstants.Users.Manage)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserCreateDto dto)
    {
        return Ok(await service.Create(dto));
    }
}