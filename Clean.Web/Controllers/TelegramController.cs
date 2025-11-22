using Clean.Application.Abstractions;
using Clean.Application.Dtos.Telegram;
using Microsoft.AspNetCore.Mvc;

namespace NoteApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly IUserRepository _users;

    public TelegramController(IUserRepository users)
    {
        _users = users;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] TelegramUpdate update)
    {
        if (update.Message != null)
        {
            var chatId = update.Message.Chat.Id;
            var name = update.Message.Chat.Username ?? update.Message.Chat.FirstName;

            // user sends /start: register chatid
            if (update.Message.Text == "/start")
            {
                await _users.SaveTelegramChatId(name, chatId);
            }
        }

        return Ok();
    }
}
