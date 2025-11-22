using System.Net.Http.Json;
using Clean.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Clean.Infrastructure.Repositories;

public class TelegramBotRepository:ITelegramBotRepository
{
    private readonly string _token;
    private readonly HttpClient _client;

    public TelegramBotRepository(IConfiguration config)
    {
        _token = config["TelegramBot:Token"];
        _client = new HttpClient();
    }

    public async Task SendMessage(long chatId, string text)
    {
        var url = $"https://api.telegram.org/bot{_token}/sendMessage";
        var data = new { chat_id = chatId, text = text };
        await _client.PostAsJsonAsync(url, data);
    }
}