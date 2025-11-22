namespace Clean.Application.Dtos.Reminder;

public class ReminderCreateDto
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public int NoteId { get; set; }
    public DateTime ReminderTime { get; set; }
}