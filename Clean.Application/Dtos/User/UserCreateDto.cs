using Clean.Domain.Entities;

namespace Clean.Application.Dtos.User;

public class UserCreateDto
{
    
    public string Name { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; }
}