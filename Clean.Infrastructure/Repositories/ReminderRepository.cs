using System.Security.Claims;
using Clean.Application.Abstractions;
using Clean.Application.Dtos.Email;
using Clean.Application.Dtos.Reminder;
using Clean.Application.Filters;
using Clean.Application.Responses;
using Clean.Domain.Entities;
using Clean.Infrastructure.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace Clean.Infrastructure.Repositories;

public class ReminderRepository(DataContext context, IHttpContextAccessor httpContextAccessor,IConfiguration configuration):IReminderRepository
{
    private int? GetCurrentUserId() => int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : null;
    public async Task<PagedResponse<ReminderGetDto>> GetAll(ReminderFilter filter)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return new PagedResponse<ReminderGetDto>(new List<ReminderGetDto>(), 0, filter.PageNumber, filter.PageSize);

        var query = context.Reminders.AsQueryable();
        query = query.Where(x=>x.UserId == currentUserId.Value);
        if (String.IsNullOrWhiteSpace(filter.Body))
            query = query.Where(r=>r.Body.ToLower().Contains(filter.Body.ToLower()));
        var totalRecords = await query.CountAsync();
        var reminders = await query.Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize).ToListAsync();
        var dto = reminders.Select(r=>new ReminderGetDto()
        {
            Id = r.Id,
            Body = r.Body,
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
        
        var find = await context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUserId.Value);
        if(find == null) return new Response<ReminderGetDto>(404,"Reminder not found");
        var dto = new ReminderGetDto()
        {
            Id = find.Id,
            Body = find.Body,
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
            Body = dto.Body,
            ReminderTime = dto.ReminderTime
        };
        context.Reminders.Add(model);
        await context.SaveChangesAsync();
        var reminder = new ReminderGetDto()
        {
            Id = model.Id,
            Body = dto.Body,
            ReminderTime = model.ReminderTime
        };
        return new Response<ReminderGetDto>(200, "Reminder created", reminder);
    }

    public async Task<Response<ReminderGetDto>> Update(ReminderUpdateDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return new Response<ReminderGetDto>(404,"User not found");
        
        var find = await context.Reminders.FirstOrDefaultAsync(r => r.Id == dto.Id && r.UserId == currentUserId.Value);
        if (find == null) return new Response<ReminderGetDto>(404,"Reminder not found");
        find.ReminderTime = dto.ReminderTime;
        await context.SaveChangesAsync();
        var reminder = new ReminderGetDto()
        {
            Id = dto.Id,
            Body = dto.Body,
            ReminderTime = dto.ReminderTime
        };
        return new Response<ReminderGetDto>(200, "Reminder updated", reminder);
    }

    public async Task<Response<string>> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return new Response<string>(404,"User not found");
        var find = await context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUserId.Value);
        if (find == null) return new Response<string>(404,"Reminder not found");
        context.Reminders.Remove(find);
        await context.SaveChangesAsync();
        return new Response<string>(200, "Reminder deleted");
    }

    public async Task<Response<string>> SendEmail()
    {
        var currentUserId = GetCurrentUserId().Value;
        var reminder = await context.Reminders.Include(r=>r.User).FirstOrDefaultAsync(r =>
            r.UserId == currentUserId &&  r.ReminderTime >= DateTime.UtcNow.AddMinutes(-1) &&
                                                                            r.ReminderTime <= DateTime.UtcNow.AddMinutes(1));
        if (reminder != null)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(configuration["EmailUserName"]));
            email.To.Add(MailboxAddress.Parse(reminder.User.Email));
            email.Subject = "You Reminder";
            email.Body = new TextPart(TextFormat.Html) { Text = "It is time to " + reminder.Body };

            using var smtp = new SmtpClient();

            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await smtp.ConnectAsync(configuration.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(configuration.GetSection("EmailUserName").Value,
                configuration.GetSection("EmailPassword").Value);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
            return new Response<string>(200, "Email sent");
        }

        return new Response<string>(404, "Reminder not found");
    }
}