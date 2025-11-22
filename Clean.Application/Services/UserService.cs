using Clean.Application.Abstractions;
using Clean.Application.Dtos.User;
using Clean.Application.Filters;
using Clean.Application.Responses;

namespace Clean.Application.Services;

public class UserService(IUserRepository repository):IUserService
{
    public async Task<PagedResponse<UserGetDto>> GetAll(UserFilter filter)
    {
        return await repository.GetAll(filter);
    }

    public async Task<Response<UserGetDto>> GetById(int id)
    {
        return await repository.GetById(id);
    }

    public async Task<Response<UserGetDto>> Update(UserUpdateDto dto)
    {
        return await repository.Update(dto);
    }

    public async Task<Response<string>> Delete(int id)
    {
        return await repository.Delete(id);
    }

    public async Task SaveTelegramChatId(string username, long chatId)
    {
        await repository.SaveTelegramChatId(username, chatId);
    }
}