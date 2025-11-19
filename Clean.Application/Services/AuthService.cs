using Clean.Application.Dtos.User;
using Clean.Application.Responses;

namespace Clean.Application.Services;

public class AuthService(IAuthService service):IAuthService
{
    public async Task<Response<LoginResponseDto>> Login(LoginRequestDto dto)
    {
        var login = await service.Login(dto);
        return login;
    }
}