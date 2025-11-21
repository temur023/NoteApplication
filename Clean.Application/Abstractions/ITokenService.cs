using Clean.Domain.Entities;

namespace Clean.Application.Services;

public interface ITokenService
{
    Task<string> GenerateJwtToken(User user);
}