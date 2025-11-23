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

            // Default message - try auto-login first
            var isAuthenticated = await TryAutoLoginAsync(chatId, session);
            await _botClient.SendTextMessageAsync(
                chatId,
                "Please use a command to get started. Type /help to see available commands.",
                replyMarkup: GetMainMenuKeyboard(isAuthenticated)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling message from chat {ChatId}. Error: {Error}", chatId, ex.Message);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            try
            {
                await _botClient.SendTextMessageAsync(
                    chatId, 
                    $"❌ An error occurred: {ex.Message}\n\nPlease try again or contact support."
                );
            }
            catch
            {
                // If sending error message fails, just log it
                _logger.LogError("Failed to send error message to chat {ChatId}", chatId);
            }
        }
    }

    private async Task HandleCommandAsync(long chatId, string command, Message message)
    {
        try
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
            case "/link":
                await HandleLinkCommandAsync(chatId, parts, session);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling command '{Command}' from chat {ChatId}", command, chatId);
            try
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Error processing command: {ex.Message}\n\nPlease try again."
                );
            }
            catch
            {
                _logger.LogError("Failed to send error message for command");
            }
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
        
        // No account found - this shouldn't happen now since we auto-create
        // But just in case, show a helpful message
        await _botClient.SendTextMessageAsync(
            chatId,
            "🎉 Welcome! Your account is being set up automatically...\n\nPlease try /start again in a moment.",
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
        
        // Telegram login failed - try auto-creating account
        // This should rarely happen now since LoginByTelegram auto-creates
        await _botClient.SendTextMessageAsync(
            chatId,
            "❌ Could not access your account. Please try /start again."
        );
    }

    private async Task HandleLinkCommandAsync(long chatId, string[] parts, UserSession session)
    {
        // /link [username] [password] - link account and login
        if (parts.Length >= 3)
        {
            var username = parts[1].TrimStart('@'); // Remove @ if present
            var password = parts[2];
            
            await HandleLoginAndLinkAsync(chatId, username, password, session);
            return;
        }
        
        // /link [username] - ask for password
        if (parts.Length >= 2)
        {
            var username = parts[1].TrimStart('@'); // Remove @ if present
            session.TempData["link_username"] = username;
            session.CurrentState = "awaiting_link_password";
            await _botClient.SendTextMessageAsync(chatId, $"Please enter your password for username: {username}");
            return;
        }
        
        // /link - ask for username
        session.CurrentState = "awaiting_link_username";
        await _botClient.SendTextMessageAsync(chatId, "Please enter your username to link your account (without @):");
    }

    private async Task HandleLoginAndLinkAsync(long chatId, string username, string password, UserSession session)
    {
        await _botClient.SendTextMessageAsync(chatId, "🔐 Logging in and linking account...");

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
                
                if (response.StatusCode == 404)
                    errorMessage = $"User '{username}' not found. Please check your username or register first.";
                else if (response.StatusCode == 401)
                    errorMessage = "Invalid password. Please check your password.";
                else if (response.StatusCode == 500)
                    errorMessage = $"Server error: {response.Message}. Please check backend logs.";
                
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Login failed. {errorMessage}\n\nTry:\n/login - to login with username and password\n/help - to see all commands"
                );
                return;
            }

            var loginData = response.Data;
            _sessionService.SetToken(chatId, loginData.Token);
            session.Username = loginData.Name;
            session.Role = loginData.Role;

            // Link Telegram account
            var linked = await _apiClient.LinkTelegramAccountAsync(loginData.Name, chatId);
            
            if (linked)
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"✅ Successfully logged in and linked your Telegram account!\n\nWelcome, {loginData.Name}!\n\n💡 Next time, just use /start or /menu to login automatically!\n\nRole: {loginData.Role}",
                    replyMarkup: GetMainMenuKeyboard(true)
                );
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"✅ Logged in as {loginData.Name}, but failed to link Telegram account.\n\nYou can still use the bot, but you'll need to login each time.",
                    replyMarkup: GetMainMenuKeyboard(true)
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during login and link");
            await _botClient.SendTextMessageAsync(
                chatId,
                $"❌ Login error: {ex.Message}. Make sure the backend API is running."
            );
        }
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
        // Try auto-login first
        if (!await TryAutoLoginAsync(chatId, session))
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                "❌ Please login first using /login or link your account with /start"
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
        if (!await TryAutoLoginAsync(chatId, session))
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
        if (!await TryAutoLoginAsync(chatId, session))
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
        if (!await TryAutoLoginAsync(chatId, session))
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
        if (!await TryAutoLoginAsync(chatId, session))
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
        if (!await TryAutoLoginAsync(chatId, session))
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
                var inputUsername = text.TrimStart('@'); // Remove @ if present
                session.TempData["username"] = inputUsername;
                session.CurrentState = "awaiting_password";
                await _botClient.SendTextMessageAsync(chatId, "Please enter your password:");
                break;

            case "awaiting_password":
                var loginUsername = session.TempData["username"]?.ToString() ?? "";
                session.TempData["password"] = text;
                session.CurrentState = null;
                session.TempData.Clear();

                await HandleLoginAsync(chatId, loginUsername, text, session);
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

            case "awaiting_link_username":
                var inputLinkUsername = text.TrimStart('@'); // Remove @ if present
                session.TempData["link_username"] = inputLinkUsername;
                session.CurrentState = "awaiting_link_password";
                await _botClient.SendTextMessageAsync(chatId, $"Please enter your password for username: {inputLinkUsername}");
                break;

            case "awaiting_link_password":
                var finalLinkUsername = session.TempData["link_username"]?.ToString() ?? "";
                session.CurrentState = null;
                session.TempData.Clear();

                await HandleLoginAndLinkAsync(chatId, finalLinkUsername, text, session);
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
                    errorMessage = $"User '{username}' not found. Please check your username or register first.";
                else if (response.StatusCode == 401)
                    errorMessage = "Invalid password. Please check your password.";
                else if (response.StatusCode == 500)
                    errorMessage = $"Server error: {response.Message}. Please check backend logs.";
                
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Login failed. {errorMessage}\n\nTry:\n/login - to login again\n/help - to see all commands"
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
        if (!await TryAutoLoginAsync(chatId, session))
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

    private async Task<bool> TryAutoLoginAsync(long chatId, UserSession session)
    {
        // If already authenticated, return true
        if (session.IsAuthenticated && !string.IsNullOrEmpty(session.Token))
            return true;

        // Try Telegram auto-login
        try
        {
            var telegramLoginResponse = await _apiClient.LoginByTelegramAsync(chatId);
            
            if (telegramLoginResponse?.Data != null && telegramLoginResponse.StatusCode == 200)
            {
                var loginData = telegramLoginResponse.Data;
                _sessionService.SetToken(chatId, loginData.Token);
                session.Username = loginData.Name;
                session.Role = loginData.Role;
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Auto-login failed for chat {ChatId}", chatId);
        }

        return false;
    }

    private async Task ShowMainMenuAsync(long chatId, UserSession session)
    {
        // Try auto-login first
        var isAuthenticated = await TryAutoLoginAsync(chatId, session);
        
        var menuText = isAuthenticated
            ? $"🎯 Main Menu\n\nWelcome, {session.Username}!\n\nWhat would you like to do?"
            : "🎯 Main Menu\n\nPlease login to access your notes and reminders.\n\nUse /login to login or /start to link your account.";

        await _botClient.SendTextMessageAsync(
            chatId,
            menuText,
            replyMarkup: GetMainMenuKeyboard(isAuthenticated)
        );
    }

    private InlineKeyboardMarkup GetMainMenuKeyboard(bool isAuthenticated)
    {
        var buttons = new List<InlineKeyboardButton[]>();
        string? webAppUrl = null;
        bool webAppEnabled = false;
        
        try
        {
            webAppUrl = _configuration["WebApp:Url"];
            webAppEnabled = _configuration.GetValue<bool>("WebApp:Enabled", false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read WebApp configuration");
        }

        if (isAuthenticated)
        {
            // Add Web App button if enabled and URL is HTTPS (Telegram requirement)
            if (webAppEnabled && !string.IsNullOrEmpty(webAppUrl) && webAppUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
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
            // Add Web App button if enabled and URL is HTTPS (Telegram requirement)
            if (webAppEnabled && !string.IsNullOrEmpty(webAppUrl) && webAppUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
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
        string? webAppUrl = null;
        bool webAppEnabled = false;
        
        try
        {
            webAppUrl = _configuration["WebApp:Url"];
            webAppEnabled = _configuration.GetValue<bool>("WebApp:Enabled", false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read WebApp configuration");
        }

        // Add Web App button only if HTTPS (Telegram requirement)
        if (webAppEnabled && !string.IsNullOrEmpty(webAppUrl) && webAppUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
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
        string? webAppUrl = null;
        bool webAppEnabled = false;
        
        try
        {
            webAppUrl = _configuration["WebApp:Url"];
            webAppEnabled = _configuration.GetValue<bool>("WebApp:Enabled", false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read WebApp configuration");
        }

        // Add Web App button only if HTTPS (Telegram requirement)
        if (webAppEnabled && !string.IsNullOrEmpty(webAppUrl) && webAppUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
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
                if (!await TryAutoLoginAsync(chatId, session))
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

