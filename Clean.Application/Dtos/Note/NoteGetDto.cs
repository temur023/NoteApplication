namespace Clean.Application.Dtos.Notification;

public class NoteGetDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}