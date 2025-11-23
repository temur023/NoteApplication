namespace Clean.Application.Filters;

public class ReminderFilter
{
    public string? Body { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}