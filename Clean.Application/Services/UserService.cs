using Clean.Application.Abstractions;
using Clean.Application.Dtos.User;
using Clean.Application.Filters;
using Clean.Application.Responses;

namespace Clean.Application.Services;

public class UserService(IUserRepository repository):IUserService
{
    public Task<PagedResponse<UserGetDto>> GetAll(UserFilter filter)
    {
        return repository.GetAll(filter);
    }

    public Task<Response<UserGetDto>> GetById(int id)
    {
        return repository.GetById(id);
    }

    public Task<Response<UserGetDto>> Update(UserUpdateDto dto)
    {
        return repository.Update(dto);
    }

    public Task<Response<string>> Delete(int id)
    {
        return repository.Delete(id);
    }
}