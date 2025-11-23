using Clean.Application.Dtos.Reminder;
using Clean.Application.Filters;
using Clean.Application.Responses;

namespace Clean.Application.Services;

public interface IReminderService
{
    Task<PagedResponse<ReminderGetDto>> GetAll(ReminderFilter filter);
    Task<Response<ReminderGetDto>> GetById(int id);
    Task<Response<ReminderGetDto>>  Create(ReminderCreateDto dto);
    Task<Response<ReminderGetDto>> Update(ReminderUpdateDto dto);
    Task<Response<string>> Delete(int id);
    Task<Response<string>> SendEmail();
}