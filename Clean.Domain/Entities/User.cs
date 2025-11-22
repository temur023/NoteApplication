namespace Clean.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; }
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}