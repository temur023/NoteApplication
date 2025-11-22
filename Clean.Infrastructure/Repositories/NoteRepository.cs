using System.Security.Claims;
using Clean.Application.Abstractions;
using Clean.Application.Dtos.Notification;
using Clean.Application.Filters;
using Clean.Application.Responses;
using Clean.Domain.Entities;
using Clean.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Clean.Infrastructure.Repositories;

public class NoteRepository(DataContext context, IHttpContextAccessor httpContextAccessor):INoteRepository
{
    private int? GetCurrentUserId() => int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : null;
    public async Task<PagedResponse<NoteGetDto>> GetAll(NoteFilter filter)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return new PagedResponse<NoteGetDto>(new List<NoteGetDto>(), 0, filter.PageNumber, filter.PageSize);

        var query = context.Notes.AsQueryable();
        query = query.Where(n => n.UserId == currentUserId.Value);
        
        if (!string.IsNullOrWhiteSpace(filter.Title))
            query = query.Where(x => x.Title.ToLower().Contains(filter.Title.ToLower()));
        if (!string.IsNullOrWhiteSpace(filter.Content))
            query = query.Where(x => x.Content.ToLower().Contains(filter.Content.ToLower()));

        var total = await query.CountAsync();
        var notes = await query.Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dto = notes.Select(n => new NoteGetDto
        {
            Id = n.Id,
            CreatedAt = n.CreatedAt.ToLocalTime(),
            Title = n.Title,
            Content = n.Content
        }).ToList();

        return new PagedResponse<NoteGetDto>(dto, total, filter.PageNumber, filter.PageSize);
    }


    public async Task<Response<NoteGetDto>> GetById(int id)
    {
        var find = await context.Notes.FirstOrDefaultAsync(n=>n.Id == id && n.UserId == GetCurrentUserId().Value);
        if(find == null) return new Response<NoteGetDto>(404,"Note not found!");
        var dto = new NoteGetDto()
        {
            Id = find.Id,
            CreatedAt = find.CreatedAt.ToLocalTime(),
            Title = find.Title,
            Content = find.Content,
        };
        return new Response<NoteGetDto>(200,"Note Found!", dto);
    }

    public async Task<Response<NoteGetDto>> Create(NoteCreateDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return new Response<NoteGetDto>(401, "User not authenticated");
        var model = new Note()
        {
            UserId = GetCurrentUserId().Value,
            CreatedAt = DateTimeOffset.UtcNow,
            Title = dto.Title,
            Content = dto.Content,
        };
        context.Notes.Add(model);
        await context.SaveChangesAsync();
        var note = new NoteGetDto()
        {
            Id = model.Id,
            CreatedAt = model.CreatedAt.ToLocalTime(),
            Title = model.Title,
            Content = model.Content,
        };
        return new Response<NoteGetDto>(200, "Note Created!", note);
    }

    public async Task<Response<NoteGetDto>> Update(NoteUpdateDto dto)
    {
        var find = await context.Notes.FirstOrDefaultAsync(n => n.Id == dto.Id && n.UserId == GetCurrentUserId().Value);
        if(find == null) return new Response<NoteGetDto>(404,"Note not found!");
        if(!String.IsNullOrWhiteSpace(dto.Title))
            find.Title = dto.Title;
        if(!String.IsNullOrWhiteSpace(dto.Content))
            find.Content = dto.Content;
        await context.SaveChangesAsync();
        var note = new NoteGetDto()
        {
            Id = find.Id,
            CreatedAt = find.CreatedAt.ToLocalTime(),
            Title = find.Title,
            Content = find.Content,
        };
        return new Response<NoteGetDto>(200, "Note Updated!", note);
    }

    public async Task<Response<string>> Delete(int id)
    {
        var find = await context.Notes.FirstOrDefaultAsync(n=>n.Id == id &&  n.UserId == GetCurrentUserId().Value);
        if(find == null) return new Response<string>(404,"Note not found!");
        context.Notes.Remove(find);
        await context.SaveChangesAsync();
        return new Response<string>(200, "Note Deleted!");
    }
}