using Clean.TelegramBot.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Clean.TelegramBot.Handlers;

public class UpdateHandler : IUpdateHandler
{
    private readonly MessageHandler _messageHandler;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(MessageHandler messageHandler, ILogger<UpdateHandler> logger)
    {
        _messageHandler = messageHandler;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message != null)
            {
                await _messageHandler.HandleMessageAsync(update.Message);
            }
            else if (update.CallbackQuery != null)
            {
                await _messageHandler.HandleCallbackQueryAsync(update.CallbackQuery);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update");
        }
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource errorSource, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Error occurred from source: {ErrorSource}", errorSource);
        await Task.CompletedTask;
    }
}

