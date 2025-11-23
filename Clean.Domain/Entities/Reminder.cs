namespace Clean.Domain.Entities;

public class Reminder
{
    public int Id { get; set; }

    public string Body { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime ReminderTime { get; set; }
}