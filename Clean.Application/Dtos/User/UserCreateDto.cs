using Clean.Domain.Entities;

namespace Clean.Application.Dtos.User;

public class UserCreateDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }
}