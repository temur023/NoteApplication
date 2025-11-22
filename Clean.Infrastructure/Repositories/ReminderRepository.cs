using System.Security.Claims;
using Clean.Application.Abstractions;
using Clean.Application.Dtos.Reminder;
using Clean.Application.Filters;
using Clean.Application.Responses;
using Clean.Domain.Entities;
using Clean.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Clean.Infrastructure.Repositories;

public class ReminderRepository(DataContext context, IHttpContextAccessor httpContextAccessor):IReminderRepository
{
    private int? GetCurrentUserId() => int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : null;
    public async Task<PagedResponse<ReminderGetDto>> GetAll(ReminderFilter filter)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return new PagedResponse<ReminderGetDto>(new List<ReminderGetDto>(), 0, filter.PageNumber, filter.PageSize);

        var query = context.Reminders.AsQueryable();
        query = query.Where(x=>x.Note.UserId == currentUserId.Value);
        if(filter.NoteId.HasValue)
            query = query.Where(x => x.NoteId == filter.NoteId.Value);
        var totalRecords = await query.CountAsync();
        var reminders = await query.Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize).ToListAsync();
        var dto = reminders.Select(r=>new ReminderGetDto()
        {
            Id = r.Id,
            NoteId = r.NoteId,
            ReminderTime = r.ReminderTime
        }).ToList();
        return new PagedResponse<ReminderGetDto>(
            dto,
            totalRecords,
            filter.PageNumber,
            filter.PageSize
            );
    }

    public async Task<Response<ReminderGetDto>> GetById(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return new Response<ReminderGetDto>(404,"User not found");
        
        var find = await context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.Note.UserId == currentUserId.Value);
        if(find == null) return new Response<ReminderGetDto>(404,"Reminder not found");
        var dto = new ReminderGetDto()
        {
            Id = find.Id,
            NoteId = find.NoteId,
            ReminderTime = find.ReminderTime
        };
        return new Response<ReminderGetDto>(200,"Reminder found", dto);
    }

    public async Task<Response<ReminderGetDto>> Create(ReminderCreateDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return new Response<ReminderGetDto>(404,"User not found");
        
        var model = new Reminder()
        {
            UserId = currentUserId.Value,
            NoteId = dto.NoteId,
            ReminderTime = dto.ReminderTime
        };
        context.Reminders.Add(model);
        await context.SaveChangesAsync();
        var reminder = new ReminderGetDto()
        {
            Id = model.Id,
            NoteId = model.NoteId,
            ReminderTime = model.ReminderTime
        };
        return new Response<ReminderGetDto>(200, "Reminder created", reminder);
    }

    public async Task<Response<ReminderGetDto>> Update(ReminderUpdateDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return new Response<ReminderGetDto>(404,"User not found");
        
        var find = await context.Reminders.FirstOrDefaultAsync(r => r.Id == dto.Id && r.Note.UserId == currentUserId.Value);
        if (find == null) return new Response<ReminderGetDto>(404,"Reminder not found");
        find.ReminderTime = dto.ReminderTime;
        await context.SaveChangesAsync();
        var reminder = new ReminderGetDto()
        {
            Id = dto.Id,
            NoteId = find.NoteId,
            ReminderTime = dto.ReminderTime
        };
        return new Response<ReminderGetDto>(200, "Reminder updated", reminder);
    }

    public async Task<Response<string>> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return new Response<string>(404,"User not found");
        var find = await context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.Note.UserId == currentUserId.Value);
        if (find == null) return new Response<string>(404,"Reminder not found");
        context.Reminders.Remove(find);
        await context.SaveChangesAsync();
        return new Response<string>(200, "Reminder deleted");
    }
}