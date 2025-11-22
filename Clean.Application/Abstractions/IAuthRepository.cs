using Clean.Application.Dtos.User;
using Clean.Application.Responses;

namespace Clean.Application.Abstractions;

public interface IAuthRepository
{
    Task<Response<LoginResponseDto>> Login(LoginRequestDto dto);
    Task<Response<UserGetDto>> Create(UserCreateDto dto);
}