namespace Clean.Application.Abstractions;

public interface ITelegramBotRepository
{
    Task SendMessage(long chatId, string text);
}