using Clean.Application.Abstractions;
using Clean.Application.Dtos.Reminder;
using Clean.Application.Filters;
using Clean.Application.Responses;

namespace Clean.Application.Services;

public class ReminderService(IReminderRepository repository):IReminderService
{
    public async Task<PagedResponse<ReminderGetDto>> GetAll(ReminderFilter filter)
    {
        return await repository.GetAll(filter);
    }

    public async Task<Response<ReminderGetDto>> GetById(int id)
    {
        return await repository.GetById(id);
    }

    public async Task<Response<ReminderGetDto>> Create(ReminderCreateDto dto)
    {
        return await repository.Create(dto);
    }

    public async Task<Response<ReminderGetDto>> Update(ReminderUpdateDto dto)
    {
        return await repository.Update(dto);
    }

    public async Task<Response<string>> Delete(int id)
    {
        return await repository.Delete(id);
    }
    
}