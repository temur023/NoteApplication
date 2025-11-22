using System.Collections.Concurrent;

namespace Clean.TelegramBot.Services;

public class UserSessionService
{
    private readonly ConcurrentDictionary<long, UserSession> _sessions = new();

    public UserSession GetOrCreateSession(long chatId)
    {
        return _sessions.GetOrAdd(chatId, _ => new UserSession { ChatId = chatId });
    }

    public void SetToken(long chatId, string token)
    {
        var session = GetOrCreateSession(chatId);
        session.Token = token;
        session.IsAuthenticated = true;
    }

    public void ClearSession(long chatId)
    {
        _sessions.TryRemove(chatId, out _);
    }

    public UserSession? GetSession(long chatId)
    {
        return _sessions.TryGetValue(chatId, out var session) ? session : null;
    }
}

public class UserSession
{
    public long ChatId { get; set; }
    public string? Token { get; set; }
    public bool IsAuthenticated { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
    public string? CurrentState { get; set; }
    public Dictionary<string, object> TempData { get; set; } = new();
}

