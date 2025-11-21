using Clean.Application.Abstractions;
using Clean.Application.Dtos.Notification;
using Clean.Application.Filters;
using Clean.Application.Responses;

namespace Clean.Application.Services;

public class NoteService(INoteRepository repository):INoteService
{
    public Task<PagedResponse<NoteGetDto>> GetAll(NoteFilter filter)
    {
        return repository.GetAll(filter);
    }

    public Task<Response<NoteGetDto>> GetById(int id)
    {
        return repository.GetById(id);
    }

    public Task<Response<NoteGetDto>> Create(NoteCreateDto dto)
    {
        return repository.Create(dto);
    }

    public Task<Response<NoteGetDto>> Update(NoteUpdateDto dto)
    {
        return repository.Update(dto);
    }

    public Task<Response<string>> Delete(int id)
    {
        return repository.Delete(id);
    }
}