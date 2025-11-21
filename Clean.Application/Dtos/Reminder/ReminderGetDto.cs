namespace Clean.Application.Dtos.Reminder;

public class ReminderGetDto
{
    public int Id { get; set; }
    public int NoteId { get; set; }
    public DateTime ReminderTime { get; set; }
}