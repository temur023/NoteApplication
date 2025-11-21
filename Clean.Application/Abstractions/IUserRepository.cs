using Clean.Application.Dtos.User;
using Clean.Application.Filters;
using Clean.Application.Responses;

namespace Clean.Application.Abstractions;

public interface IUserRepository
{
    Task<UserGetDto> GetAll(UserFilter filter);
    Task<UserGetDto> GetById(int id);
    Task<UserGetDto> Update(UserCreateDto dto);
    Task<string> Delete(int id);
    Task<UserUpdateRoleDto> UpdateRole(UserUpdateRoleDto dto);
}