using Clean.TelegramBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Clean.TelegramBot.Handlers;

public class MessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ApiClient _apiClient;
    private readonly UserSessionService _sessionService;
    private readonly ILogger<MessageHandler> _logger;
    private readonly IConfiguration _configuration;

    public MessageHandler(
        ITelegramBotClient botClient,
        ApiClient apiClient,
        UserSessionService sessionService,
        ILogger<MessageHandler> logger,
        IConfiguration configuration)
    {
        _botClient = botClient;
        _apiClient = apiClient;
        _sessionService = sessionService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task HandleMessageAsync(Message message)
    {
        if (message.Text == null) return;

        var chatId = message.Chat.Id;
        var session = _sessionService.GetOrCreateSession(chatId);
        var text = message.Text.Trim();

        try
        {
            // Handle commands
            if (text.StartsWith("/"))
            {
                await HandleCommandAsync(chatId, text, message);
                return;
            }

            // Handle state-based inputs
            if (!string.IsNullOrEmpty(session.CurrentState))
            {
                await HandleStateInputAsync(chatId, text, session);
                return;
            }

            // Default message
            await _botClient.SendTextMessageAsync(
                chatId,
                "Please use a command to get started. Type /help to see available commands.",
                replyMarkup: GetMainMenuKeyboard(session.IsAuthenticated)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling message from chat {ChatId}", chatId);
            await _botClient.SendTextMessageAsync(chatId, "An error occurred. Please try again.");
        }
    }

    private async Task HandleCommandAsync(long chatId, string command, Message message)
    {
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToLower();
        var session = _sessionService.GetOrCreateSession(chatId);

        switch (cmd)
        {
            case "/start":
                await HandleStartCommandAsync(chatId, message);
                break;
            case "/login":
                await HandleLoginCommandAsync(chatId);
                break;
            case "/logout":
                await HandleLogoutCommandAsync(chatId, session);
                break;
            case "/help":
                await HandleHelpCommandAsync(chatId, session);
                break;
            case "/notes":
                await HandleNotesCommandAsync(chatId, session);
                break;
            case "/reminders":
                await HandleRemindersCommandAsync(chatId, session);
                break;
            case "/menu":
                await ShowMainMenuAsync(chatId, session);
                break;
            default:
                if (parts.Length > 1 && int.TryParse(parts[1], out var id))
                {
                    if (cmd == "/note")
                        await HandleViewNoteCommandAsync(chatId, session, id);
                    else if (cmd == "/reminder")
                        await HandleViewReminderCommandAsync(chatId, session, id);
                    else if (cmd == "/deletenote")
                        await HandleDeleteNoteCommandAsync(chatId, session, id);
                    else if (cmd == "/deletereminder")
                        await HandleDeleteReminderCommandAsync(chatId, session, id);
                }
                break;
        }
    }

    private async Task HandleStartCommandAsync(long chatId, Message message)
    {
        var session = _sessionService.GetOrCreateSession(chatId);
        
        // Try automatic Telegram login first
        await _botClient.SendTextMessageAsync(chatId, "🔍 Checking your account...");
        
        var telegramLoginResponse = await _apiClient.LoginByTelegramAsync(chatId);
        
        if (telegramLoginResponse?.Data != null && telegramLoginResponse.StatusCode == 200)
        {
            // Successfully logged in via Telegram!
            var loginData = telegramLoginResponse.Data;
            _sessionService.SetToken(chatId, loginData.Token);
            session.Username = loginData.Name;
            session.Role = loginData.Role;
            
            var welcomeMessage = $@"
✅ Welcome back, {loginData.Name}!

You're automatically logged in using your Telegram account.

📋 Available Commands:
/menu - Show main menu
/notes - List your notes
/reminders - List your reminders
/help - Show help information

Role: {loginData.Role}";
            
            await _botClient.SendTextMessageAsync(
                chatId,
                welcomeMessage,
                replyMarkup: GetMainMenuKeyboard(true)
            );
            return;
        }
        
        // No account linked, show welcome message
        var welcomeMessage2 = @"
🎉 Welcome to Note App Bot!

This bot helps you manage your notes and reminders.

📋 To get started:
1. Link your Telegram account with /link [username]
   (where username is your app account username)
2. Or login with /login [username] [password]

📋 Available Commands:
/start - Show this welcome message
/login - Login to your account
/link - Link your Telegram account
/help - Show help information";
        
        await _botClient.SendTextMessageAsync(
            chatId,
            welcomeMessage2,
            replyMarkup: GetMainMenuKeyboard(false)
        );
    }

    private async Task HandleLoginCommandAsync(long chatId)
    {
        var session = _sessionService.GetOrCreateSession(chatId);
        
        // First try Telegram login automatically
        await _botClient.SendTextMessageAsync(chatId, "🔍 Trying automatic login...");
        
        var telegramLoginResponse = await _apiClient.LoginByTelegramAsync(chatId);
        
        if (telegramLoginResponse?.Data != null && telegramLoginResponse.StatusCode == 200)
        {
            // Successfully logged in via Telegram!
            var loginData = telegramLoginResponse.Data;
            _sessionService.SetToken(chatId, loginData.Token);
            session.Username = loginData.Name;
            session.Role = loginData.Role;
            
            await _botClient.SendTextMessageAsync(
                chatId,
                $"✅ Automatically logged in as {loginData.Name}!\n\nUse /menu to see available options.",
                replyMarkup: GetMainMenuKeyboard(true)
            );
            return;
        }
        
        // Telegram login failed, ask for username/password
        session.CurrentState = "awaiting_username";
        
        await _botClient.SendTextMessageAsync(
            chatId,
            "❌ No account linked to this Telegram account.\n\nPlease enter your username (or use /link [username] to link your account):"
        );
    }

    private async Task HandleLogoutCommandAsync(long chatId, UserSession session)
    {
        _sessionService.ClearSession(chatId);
        await _botClient.SendTextMessageAsync(
            chatId,
            "✅ You have been logged out successfully.",
            replyMarkup: GetMainMenuKeyboard(false)
        );
    }

    private async Task HandleHelpCommandAsync(long chatId, UserSession session)
    {
        var helpText = @"
📚 Help - Available Commands:

🔐 Authentication:
/login - Login to your account
/logout - Logout from your account

📝 Notes:
/notes - List all your notes
/note [id] - View a specific note
/create - Create a new note (after login)
/update - Update a note (after login)
/deletenote [id] - Delete a note

🔔 Reminders:
/reminders - List all your reminders
/reminder [id] - View a specific reminder
/createreminder - Create a new reminder
/deletereminder [id] - Delete a reminder

🛠️ General:
/start - Start the bot
/help - Show this help
/menu - Show main menu";

        await _botClient.SendTextMessageAsync(
            chatId,
            helpText,
            replyMarkup: GetMainMenuKeyboard(session.IsAuthenticated)
        );
    }

    private async Task HandleNotesCommandAsync(long chatId, UserSession session)
    {
        if (!session.IsAuthenticated || string.IsNullOrEmpty(session.Token))
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                "❌ Please login first using /login"
            );
            return;
        }

        var response = await _apiClient.GetNotesAsync(session.Token, 1, 10);
        
        if (response?.Data == null || response.StatusCode != 200)
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                $"❌ Failed to fetch notes. {response?.Message ?? "Unknown error"}"
            );
            return;
        }

        var notes = response.Data;
        if (notes.Count == 0)
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                "📝 You have no notes yet. Use /create to create one.",
                replyMarkup: GetNotesMenuKeyboard()
            );
            return;
        }

        var notesText = "📝 Your Notes:\n\n";
        foreach (var note in notes)
        {
            notesText += $"📄 Note #{note.Id}\n";
            notesText += $"Title: {note.Title}\n";
            notesText += $"Created: {note.CreatedAt:yyyy-MM-dd HH:mm}\n";
            notesText += $"/note {note.Id} - View details\n\n";
        }

        await _botClient.SendTextMessageAsync(
            chatId,
            notesText,
            replyMarkup: GetNotesMenuKeyboard()
        );
    }

    private async Task HandleViewNoteCommandAsync(long chatId, UserSession session, int noteId)
    {
        if (!session.IsAuthenticated || string.IsNullOrEmpty(session.Token))
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Please login first using /login");
            return;
        }

        var response = await _apiClient.GetNoteByIdAsync(session.Token, noteId);
        
        if (response?.Data == null || response.StatusCode != 200)
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                $"❌ Failed to fetch note. {response?.Message ?? "Note not found"}"
            );
            return;
        }

        var note = response.Data;
        var noteText = $"📄 Note #{note.Id}\n\n";
        noteText += $"Title: {note.Title}\n\n";
        noteText += $"Content:\n{note.Content}\n\n";
        noteText += $"Created: {note.CreatedAt:yyyy-MM-dd HH:mm:ss}\n\n";
        noteText += $"Actions:\n";
        noteText += $"/deletenote {note.Id} - Delete this note";

        await _botClient.SendTextMessageAsync(
            chatId,
            noteText,
            replyMarkup: GetNotesMenuKeyboard()
        );
    }

    private async Task HandleRemindersCommandAsync(long chatId, UserSession session)
    {
        if (!session.IsAuthenticated || string.IsNullOrEmpty(session.Token))
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Please login first using /login");
            return;
        }

        var response = await _apiClient.GetRemindersAsync(session.Token, 1, 10);
        
        if (response?.Data == null || response.StatusCode != 200)
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                $"❌ Failed to fetch reminders. {response?.Message ?? "Unknown error"}"
            );
            return;
        }

        var reminders = response.Data;
        if (reminders.Count == 0)
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                "🔔 You have no reminders yet.",
                replyMarkup: GetRemindersMenuKeyboard()
            );
            return;
        }

        var remindersText = "🔔 Your Reminders:\n\n";
        foreach (var reminder in reminders)
        {
            remindersText += $"⏰ Reminder #{reminder.Id}\n";
            remindersText += $"Note ID: {reminder.NoteId}\n";
            remindersText += $"Time: {reminder.ReminderTime:yyyy-MM-dd HH:mm}\n";
            remindersText += $"/reminder {reminder.Id} - View details\n\n";
        }

        await _botClient.SendTextMessageAsync(
            chatId,
            remindersText,
            replyMarkup: GetRemindersMenuKeyboard()
        );
    }

    private async Task HandleViewReminderCommandAsync(long chatId, UserSession session, int reminderId)
    {
        if (!session.IsAuthenticated || string.IsNullOrEmpty(session.Token))
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Please login first using /login");
            return;
        }

        var response = await _apiClient.GetReminderByIdAsync(session.Token, reminderId);
        
        if (response?.Data == null || response.StatusCode != 200)
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                $"❌ Failed to fetch reminder. {response?.Message ?? "Reminder not found"}"
            );
            return;
        }

        var reminder = response.Data;
        var reminderText = $"🔔 Reminder #{reminder.Id}\n\n";
        reminderText += $"Note ID: {reminder.NoteId}\n";
        reminderText += $"Time: {reminder.ReminderTime:yyyy-MM-dd HH:mm:ss}\n\n";
        reminderText += $"Actions:\n";
        reminderText += $"/deletereminder {reminder.Id} - Delete this reminder";

        await _botClient.SendTextMessageAsync(
            chatId,
            reminderText,
            replyMarkup: GetRemindersMenuKeyboard()
        );
    }

    private async Task HandleDeleteNoteCommandAsync(long chatId, UserSession session, int noteId)
    {
        if (!session.IsAuthenticated || string.IsNullOrEmpty(session.Token))
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Please login first using /login");
            return;
        }

        var success = await _apiClient.DeleteNoteAsync(session.Token, noteId);
        
        if (success)
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                "✅ Note deleted successfully!",
                replyMarkup: GetNotesMenuKeyboard()
            );
        }
        else
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                "❌ Failed to delete note. Please try again."
            );
        }
    }

    private async Task HandleDeleteReminderCommandAsync(long chatId, UserSession session, int reminderId)
    {
        if (!session.IsAuthenticated || string.IsNullOrEmpty(session.Token))
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Please login first using /login");
            return;
        }

        var success = await _apiClient.DeleteReminderAsync(session.Token, reminderId);
        
        if (success)
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                "✅ Reminder deleted successfully!",
                replyMarkup: GetRemindersMenuKeyboard()
            );
        }
        else
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                "❌ Failed to delete reminder. Please try again."
            );
        }
    }

    private async Task HandleStateInputAsync(long chatId, string text, UserSession session)
    {
        switch (session.CurrentState)
        {
            case "awaiting_username":
                session.TempData["username"] = text;
                session.CurrentState = "awaiting_password";
                await _botClient.SendTextMessageAsync(chatId, "Please enter your password:");
                break;

            case "awaiting_password":
                var username = session.TempData["username"]?.ToString() ?? "";
                session.TempData["password"] = text;
                session.CurrentState = null;
                session.TempData.Clear();

                await HandleLoginAsync(chatId, username, text, session);
                break;

            case "awaiting_note_title":
                session.TempData["note_title"] = text;
                session.CurrentState = "awaiting_note_content";
                await _botClient.SendTextMessageAsync(chatId, "Please enter the note content:");
                break;

            case "awaiting_note_content":
                var noteTitle = session.TempData["note_title"]?.ToString() ?? "";
                session.CurrentState = null;
                session.TempData.Clear();

                await HandleCreateNoteAsync(chatId, noteTitle, text, session);
                break;
        }
    }

    private async Task HandleLoginAsync(long chatId, string username, string password, UserSession session)
    {
        await _botClient.SendTextMessageAsync(chatId, "🔐 Logging in...");

        try
        {
            var response = await _apiClient.LoginAsync(username, password);
            
            if (response == null)
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "❌ Login failed. Could not connect to the backend API. Make sure the API is running at http://localhost:5143"
                );
                return;
            }
            
            if (response.Data == null || response.StatusCode != 200)
            {
                var errorMessage = response.Message ?? "Invalid credentials";
                
                // More specific error messages
                if (response.StatusCode == 404)
                    errorMessage = "User not found";
                else if (response.StatusCode == 401)
                    errorMessage = "Invalid password";
                else if (response.StatusCode == 500)
                    errorMessage = "Server error. Check backend logs.";
                
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Login failed. {errorMessage}"
                );
                return;
            }

            var loginData = response.Data;
            _sessionService.SetToken(chatId, loginData.Token);
            session.Username = loginData.Name;
            session.Role = loginData.Role;

            // Automatically link Telegram account after successful login
            var linked = await _apiClient.LinkTelegramAccountAsync(loginData.Name, chatId);
            
            if (linked)
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"✅ Welcome, {loginData.Name}! You are now logged in.\n\n✅ Your Telegram account has been linked.\n\nRole: {loginData.Role}\n\nUse /menu to see available options.\n\n💡 Next time, you can use /start to login automatically!",
                    replyMarkup: GetMainMenuKeyboard(true)
                );
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"✅ Welcome, {loginData.Name}! You are now logged in.\n\nRole: {loginData.Role}\n\nUse /menu to see available options.",
                    replyMarkup: GetMainMenuKeyboard(true)
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during login");
            await _botClient.SendTextMessageAsync(
                chatId,
                $"❌ Login error: {ex.Message}. Make sure the backend API is running."
            );
        }
    }

    private async Task HandleCreateNoteAsync(long chatId, string title, string content, UserSession session)
    {
        if (!session.IsAuthenticated || string.IsNullOrEmpty(session.Token))
        {
            await _botClient.SendTextMessageAsync(chatId, "❌ Please login first using /login");
            return;
        }

        await _botClient.SendTextMessageAsync(chatId, "📝 Creating note...");

        var note = new Models.NoteCreateDto { Title = title, Content = content };
        var response = await _apiClient.CreateNoteAsync(session.Token, note);
        
        if (response?.Data == null || response.StatusCode != 200)
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                $"❌ Failed to create note. {response?.Message ?? "Unknown error"}"
            );
            return;
        }

        var createdNote = response.Data;
        await _botClient.SendTextMessageAsync(
            chatId,
            $"✅ Note created successfully!\n\n📄 Note #{createdNote.Id}\nTitle: {createdNote.Title}\n\nUse /note {createdNote.Id} to view it.",
            replyMarkup: GetNotesMenuKeyboard()
        );
    }

    private async Task ShowMainMenuAsync(long chatId, UserSession session)
    {
        var menuText = session.IsAuthenticated
            ? $"🎯 Main Menu\n\nWelcome, {session.Username}!\n\nWhat would you like to do?"
            : "🎯 Main Menu\n\nPlease login to access your notes and reminders.";

        await _botClient.SendTextMessageAsync(
            chatId,
            menuText,
            replyMarkup: GetMainMenuKeyboard(session.IsAuthenticated)
        );
    }

    private InlineKeyboardMarkup GetMainMenuKeyboard(bool isAuthenticated)
    {
        var buttons = new List<InlineKeyboardButton[]>();
        var webAppUrl = _configuration["WebApp:Url"];
        var webAppEnabled = _configuration.GetValue<bool>("WebApp:Enabled", false);

        if (isAuthenticated)
        {
            // Add Web App button if enabled
            if (webAppEnabled && !string.IsNullOrEmpty(webAppUrl))
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithWebApp("🚀 Open App", new Telegram.Bot.Types.WebAppInfo { Url = webAppUrl })
                });
            }
            
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("📝 Notes", "notes"),
                InlineKeyboardButton.WithCallbackData("🔔 Reminders", "reminders")
            });
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("➕ Create Note", "create_note"),
                InlineKeyboardButton.WithCallbackData("❌ Logout", "logout")
            });
        }
        else
        {
            // Add Web App button if enabled (login will be handled in the web app)
            if (webAppEnabled && !string.IsNullOrEmpty(webAppUrl))
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithWebApp("🚀 Open App", new Telegram.Bot.Types.WebAppInfo { Url = webAppUrl })
                });
            }
            
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("🔐 Login", "login"),
                InlineKeyboardButton.WithCallbackData("❓ Help", "help")
            });
        }

        return new InlineKeyboardMarkup(buttons);
    }

    private InlineKeyboardMarkup GetNotesMenuKeyboard()
    {
        var buttons = new List<InlineKeyboardButton[]>();
        var webAppUrl = _configuration["WebApp:Url"];
        var webAppEnabled = _configuration.GetValue<bool>("WebApp:Enabled", false);

        if (webAppEnabled && !string.IsNullOrEmpty(webAppUrl))
        {
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithWebApp("🚀 Open in App", new Telegram.Bot.Types.WebAppInfo { Url = webAppUrl })
            });
        }

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("📋 List Notes", "notes"),
            InlineKeyboardButton.WithCallbackData("➕ Create Note", "create_note")
        });
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🏠 Main Menu", "menu")
        });

        return new InlineKeyboardMarkup(buttons);
    }

    private InlineKeyboardMarkup GetRemindersMenuKeyboard()
    {
        var buttons = new List<InlineKeyboardButton[]>();
        var webAppUrl = _configuration["WebApp:Url"];
        var webAppEnabled = _configuration.GetValue<bool>("WebApp:Enabled", false);

        if (webAppEnabled && !string.IsNullOrEmpty(webAppUrl))
        {
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithWebApp("🚀 Open in App", new Telegram.Bot.Types.WebAppInfo { Url = webAppUrl })
            });
        }

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🔔 List Reminders", "reminders"),
            InlineKeyboardButton.WithCallbackData("➕ Create Reminder", "create_reminder")
        });
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🏠 Main Menu", "menu")
        });

        return new InlineKeyboardMarkup(buttons);
    }

    public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
    {
        var chatId = callbackQuery.Message!.Chat.Id;
        var data = callbackQuery.Data;
        var session = _sessionService.GetOrCreateSession(chatId);

        await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

        switch (data)
        {
            case "menu":
                await ShowMainMenuAsync(chatId, session);
                break;
            case "notes":
                await HandleNotesCommandAsync(chatId, session);
                break;
            case "reminders":
                await HandleRemindersCommandAsync(chatId, session);
                break;
            case "create_note":
                if (!session.IsAuthenticated)
                {
                    await _botClient.SendTextMessageAsync(chatId, "❌ Please login first using /login");
                    return;
                }
                session.CurrentState = "awaiting_note_title";
                await _botClient.SendTextMessageAsync(chatId, "Please enter the note title:");
                break;
            case "login":
                await HandleLoginCommandAsync(chatId);
                break;
            case "logout":
                await HandleLogoutCommandAsync(chatId, session);
                break;
            case "help":
                await HandleHelpCommandAsync(chatId, session);
                break;
        }
    }
}

