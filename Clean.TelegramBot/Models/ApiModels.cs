namespace Clean.TelegramBot.Models;

public class LoginRequest
{
    public string Name { get; set; } = string.Empty;
    public string UserPassword { get; set; } = string.Empty;
}

public class TelegramLoginRequest
{
    public long ChatId { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class PagedResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<T> Data { get; set; } = new();
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class NoteCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class NoteUpdateDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
}

public class NoteGetDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public class ReminderCreateDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int NoteId { get; set; }
    public DateTime ReminderTime { get; set; }
}

public class ReminderUpdateDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime ReminderTime { get; set; }
}

public class ReminderGetDto
{
    public int Id { get; set; }
    public int NoteId { get; set; }
    public DateTime ReminderTime { get; set; }
}

