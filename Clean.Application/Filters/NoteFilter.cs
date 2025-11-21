namespace Clean.Application.Filters;

public class NoteFilter
{
    public DateOnly? Date { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}