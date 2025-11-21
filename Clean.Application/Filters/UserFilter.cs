namespace Clean.Application.Filters;

public class UserFilter
{
    public string? Name { get; set; }
    public string? Role { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}