namespace Clean.Domain.Entities;

public class Reminder
{
    public int Id { get; set; }
    public Note Note { get; set; }
    public int NoteId { get; set; }
    public DateTime ReminderTime { get; set; }
}