using Clean.TelegramBot.Handlers;
using Clean.TelegramBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services
builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var token = builder.Configuration["Telegram:BotToken"];
    if (string.IsNullOrEmpty(token))
    {
        throw new InvalidOperationException("Telegram Bot Token is not configured. Please set it in appsettings.json");
    }
    return new TelegramBotClient(token);
});

builder.Services.AddHttpClient<ApiClient>();
builder.Services.AddSingleton<ApiClient>();
builder.Services.AddSingleton<UserSessionService>();
builder.Services.AddSingleton<MessageHandler>();
builder.Services.AddSingleton<UpdateHandler>();

// Add logging
builder.Services.AddLogging(configure => configure.AddConsole());

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var botClient = host.Services.GetRequiredService<ITelegramBotClient>();
var updateHandler = host.Services.GetRequiredService<UpdateHandler>();

// Configure bot handlers
using var cts = new CancellationTokenSource();

try
{
    var receiverOptions = new ReceiverOptions
    {
        AllowedUpdates = new[] 
        { 
            Telegram.Bot.Types.Enums.UpdateType.Message,
            Telegram.Bot.Types.Enums.UpdateType.CallbackQuery 
        }
    };

    botClient.StartReceiving(
        updateHandler: updateHandler,
        receiverOptions: receiverOptions,
        cancellationToken: cts.Token
    );

    var me = await botClient.GetMe(cts.Token);
    logger.LogInformation("Bot @{BotUsername} started successfully!", me.Username);
}
catch (Exception ex)
{
    logger.LogError(ex, "Error starting bot");
    throw;
}

logger.LogInformation("Bot is running. Press Ctrl+C to stop.");

await host.RunAsync(cts.Token);

