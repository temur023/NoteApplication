using Clean.Application.Dtos.User;
using Clean.Application.Responses;

namespace Clean.Application.Services;

public interface IAuthService
{
    Task<Response<LoginResponseDto>> Login(LoginRequestDto dto);
    Task<Response<UserGetDto>> Create(UserCreateDto dto);
}