using Clean.Application.Abstractions;
using Clean.Application.Dtos.User;
using Clean.Application.Responses;

namespace Clean.Application.Services;

public class AuthService(IAuthRepository service):IAuthService
{
    public async Task<Response<LoginResponseDto>> Login(LoginRequestDto dto)
    {
        var login = await service.Login(dto);
        return login;
    }

    public async Task<Response<UserGetDto>> Create(UserCreateDto dto)
    {
        var create = await service.Create(dto);
        return create;
    }
}