using Clean.Application.Dtos.Notification;
using Clean.Application.Filters;
using Clean.Application.Responses;

namespace Clean.Application.Abstractions;

public interface INoteRepository
{
    Task<PagedResponse<NoteGetDto>> GetAll(NoteFilter filter);
    Task<Response<NoteGetDto>> GetById(int id);
    Task<Response<NoteGetDto>>  Create(NoteCreateDto dto);
    Task<Response<NoteGetDto>> Update(NoteUpdateDto dto);
    Task<Response<string>> Delete(int id);
}