using Clean.Application.Abstractions;
using Clean.Application.Dtos.Notification;
using Clean.Application.Filters;
using Clean.Application.Responses;
using Clean.Domain.Entities;
using Clean.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clean.Infrastructure.Repositories;

public class NoteRepository(DataContext context):INoteRepository
{
    public async Task<PagedResponse<NoteGetDto>> GetAll(NoteFilter filter)
    {
        var query = context.Notes.AsQueryable();
        if(filter.Date.HasValue)
            query = query.Where(x => x.CreatedAt == filter.Date.Value);
        if(!String.IsNullOrWhiteSpace(filter.Title))
            query = query.Where(x => x.Title.ToLower().Contains(filter.Title.ToLower()));
        if(!String.IsNullOrWhiteSpace(filter.Content))
            query = query.Where(x => x.Content.ToLower().Contains(filter.Content.ToLower()));
        var total = await query.CountAsync();
        var notes = await query.Skip((filter.PageNumber-1)*filter.PageSize)
            .Take(filter.PageSize).ToListAsync();
        var dto = notes.Select(n => new NoteGetDto()
        {
            CreatedAt = n.CreatedAt,
            Title = n.Title,
            Content = n.Content,
        }).ToList();
        return new PagedResponse<NoteGetDto>(dto, total, filter.PageNumber, filter.PageSize);
    }

    public async Task<Response<NoteGetDto>> GetById(int id)
    {
        var find = await context.Notes.FirstOrDefaultAsync(n=>n.Id == id);
        if(find == null) return new Response<NoteGetDto>(404,"Note not found!");
        var dto = new NoteGetDto()
        {
            CreatedAt = find.CreatedAt,
            Title = find.Title,
            Content = find.Content,
        };
        return new Response<NoteGetDto>(200,"Note Found!", dto);
    }

    public async Task<Response<NoteGetDto>> Create(NoteCreateDto dto)
    {
        var model = new Note()
        {
            CreatedAt = DateOnly.FromDateTime(DateTime.Now),
            Title = dto.Title,
            Content = dto.Content,
        };
        context.Notes.Add(model);
        await context.SaveChangesAsync();
        var note = new NoteGetDto()
        {
            CreatedAt = model.CreatedAt,
            Title = model.Title,
            Content = model.Content,
        };
        return new Response<NoteGetDto>(200, "Note Created!", note);
    }

    public async Task<Response<NoteGetDto>> Update(NoteUpdateDto dto)
    {
        var find = await context.Notes.FirstOrDefaultAsync(n => n.Id == dto.Id);
        if(find == null) return new Response<NoteGetDto>(404,"Note not found!");
        if(!String.IsNullOrWhiteSpace(dto.Title))
            find.Title = dto.Title;
        if(!String.IsNullOrWhiteSpace(dto.Content))
            find.Content = dto.Content;
        await context.SaveChangesAsync();
        var note = new NoteGetDto()
        {
            CreatedAt = find.CreatedAt,
            Title = find.Title,
            Content = find.Content,
        };
        return new Response<NoteGetDto>(200, "Note Updated!", note);
    }

    public async Task<Response<string>> Delete(int id)
    {
        var find = await context.Notes.FirstOrDefaultAsync(n=>n.Id == id);
        if(find == null) return new Response<string>(404,"Note not found!");
        context.Notes.Remove(find);
        await context.SaveChangesAsync();
        return new Response<string>(200, "Note Deleted!");
    }
}