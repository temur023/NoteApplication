namespace Clean.Application.Dtos.Reminder;

public class ReminderUpdateDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime ReminderTime { get; set; }
}