using Clean.Application.Abstractions;
using Clean.Domain.Entities;

namespace Clean.Application.Services;

public class TokenService(ITokenRepository service):ITokenService
{
    public async Task<string> GenerateJwtToken(User user)
    {
        var generate = await service.GenerateJwtToken(user);
        return generate;
    }
}