using Clean.Domain.Entities;

namespace Clean.Application.Dtos.User;

public class UserGetDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PassworodHash { get; set; }
    public Role Role { get; set; }
}