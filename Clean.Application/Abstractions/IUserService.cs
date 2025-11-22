using Clean.Application.Dtos.User;
using Clean.Application.Filters;
using Clean.Application.Responses;

namespace Clean.Application.Services;

public interface IUserService
{
    Task<PagedResponse<UserGetDto>> GetAll(UserFilter filter);
    Task<Response<UserGetDto>> GetById(int id);
    Task<Response<UserGetDto>> Update(UserUpdateDto dto);
    Task<Response<string>> Delete(int id);
    Task SaveTelegramChatId(string username, long chatId);

}