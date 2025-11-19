namespace Clean.Application.Dtos.User;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public string Name { get; set; }
    public string Role { get; set; } = null!;
}