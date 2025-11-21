using Clean.Domain.Entities;

namespace Clean.Application.Services;

public classTokenService(ITokenService service):ITokenService
{
    public async Task<string> GenerateJwtToken(User user)
    {
        var generate = await service.GenerateJwtToken(user);
        return generate;
    }
}