using Clean.Domain.Entities;

namespace Clean.Application.Abstractions;

public interface ITokenRepository
{
    Task<string> GenerateJwtToken(User user);
}