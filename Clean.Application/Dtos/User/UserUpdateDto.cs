using Clean.Domain.Entities;

namespace Clean.Application.Dtos.User;

public class UserUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; }
}