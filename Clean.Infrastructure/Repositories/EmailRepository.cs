namespace Clean.Infrastructure.Repositories;

using Clean.Application.Abstractions;
using Clean.Infrastructure.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

public class EmailRepository (DataContext context, IConfiguration configuration): IEmailRepository
{
    public async Task SendEmailEveryMinute()
    {
        var now = DateTime.UtcNow;

        var reminders = await context.Reminders
            .Include(r => r.User)
            .Where(r =>
                r.ReminderTime >= now.AddMinutes(-1) &&
                r.ReminderTime <= now.AddMinutes(1))
            .ToListAsync();

        if (!reminders.Any()) return;

        foreach (var reminder in reminders)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(configuration["EmailUserName"]));
            email.To.Add(MailboxAddress.Parse(reminder.User.Email));
            email.Subject = "Reminder";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $"It is time to: {reminder.Body}"
            };

            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await smtp.ConnectAsync(configuration["EmailHost"], 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(configuration["EmailUserName"], configuration["EmailPassword"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
