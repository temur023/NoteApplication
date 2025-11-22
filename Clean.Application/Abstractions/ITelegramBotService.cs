namespace Clean.Application.Abstractions;

public interface ITelegramBotService
{
    Task SendMessage(long chatId, string text);
}