using Clean.Application.Abstractions;

namespace Clean.Application.Services;

public class TelegramBotService(ITelegramBotRepository repository):ITelegramBotService
{
    public async Task SendMessage(long chatId, string text)
    {
        await repository.SendMessage(chatId, text);
    }
}