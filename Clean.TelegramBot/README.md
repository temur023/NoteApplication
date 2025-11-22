# Telegram Bot for Note Application

This is the Telegram bot frontend for the Note Application backend API.

## Features

- 🔐 **Authentication**: Login with username and password
- 📝 **Note Management**: Create, view, list, and delete notes
- 🔔 **Reminder Management**: Create, view, list, and delete reminders
- 💬 **Interactive Menus**: User-friendly inline keyboards for navigation

## Prerequisites

- .NET 9.0 SDK
- Telegram Bot Token (get it from [@BotFather](https://t.me/botfather))
- Running Note Application API (default: http://localhost:5143)

## Configuration

1. Update `appsettings.json`:
   ```json
   {
     "Telegram": {
       "BotToken": "YOUR_BOT_TOKEN_HERE"
     },
     "Api": {
       "BaseUrl": "http://localhost:5143/api"
     },
     "WebApp": {
       "Url": "https://your-domain.com",
       "Enabled": true
     }
   }
   ```

2. **For Telegram Mini App (Web App):**
   - Set `WebApp:Url` to your deployed frontend URL (MUST be HTTPS)
   - Set `WebApp:Enabled` to `true`
   - Your frontend must be served over HTTPS (Telegram requirement)
   - For local development, use tools like ngrok: `ngrok http 3000`

3. Make sure your backend API is running and accessible.

## Running the Bot

```bash
cd Clean.TelegramBot
dotnet restore
dotnet run
```

## Commands

### Authentication
- `/start` - Start the bot and see welcome message
- `/login` - Login to your account
- `/logout` - Logout from your account
- `/help` - Show help information

### Notes
- `/notes` - List all your notes
- `/note [id]` - View a specific note by ID
- `/create` - Create a new note (interactive)
- `/deletenote [id]` - Delete a note by ID

### Reminders
- `/reminders` - List all your reminders
- `/reminder [id]` - View a specific reminder by ID
- `/createreminder` - Create a new reminder
- `/deletereminder [id]` - Delete a reminder by ID

### Navigation
- `/menu` - Show main menu

## How It Works

1. **Authentication**: Users login with their username and password. The bot stores the JWT token in memory for subsequent API calls.

2. **Session Management**: Each chat session maintains:
   - Authentication token
   - User information
   - Current state (for multi-step inputs like creating notes)

3. **API Communication**: The bot communicates with the backend API using HTTP requests with Bearer token authentication.

## Telegram Mini App (Web App)

The bot now supports Telegram Mini Apps! When users press the "🚀 Open App" button, it opens your React frontend inside Telegram as a Mini App.

### Setup Mini App:

1. **Deploy your frontend with HTTPS:**
   - Telegram requires HTTPS for Mini Apps
   - You can use services like Vercel, Netlify, or your own server with SSL
   - For local testing, use ngrok: `ngrok http 3000`

2. **Update `appsettings.json`:**
   - Set `WebApp:Url` to your HTTPS frontend URL
   - Set `WebApp:Enabled` to `true`

3. **Configure your frontend for Telegram:**
   - Add the Telegram Web App script to your `index.html`:
   ```html
   <script src="https://telegram.org/js/telegram-web-app.js"></script>
   ```
   - Access Telegram user data via `window.Telegram.WebApp.initData`
   - Use `window.Telegram.WebApp.expand()` to expand the app to fullscreen

4. **Set Web App URL in BotFather:**
   - Open [@BotFather](https://t.me/botfather) on Telegram
   - Use `/newapp` or `/editapp` command
   - Select your bot
   - Set the Web App URL to match your frontend URL

## Notes

- Sessions are stored in memory and will be lost when the bot restarts
- Users need to login again after bot restart
- The bot uses long polling to receive updates from Telegram
- Make sure your backend API CORS settings allow requests from your bot (if running on different domains)
- Telegram Mini Apps require HTTPS - localhost won't work without a tunnel like ngrok

## Troubleshooting

- **Bot not responding**: Check if the bot token is correct in `appsettings.json`
- **API errors**: Ensure the backend API is running and the BaseUrl in `appsettings.json` is correct
- **Login fails**: Verify your credentials and that the backend API is accessible

