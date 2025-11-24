using Clean.Application.Abstractions;

namespace Clean.Application.Services;

public class EmailService(IEmailRepository repository):IEmailService
{
    public async Task SendEmailEveryMinute()
    {
        await repository.SendEmailEveryMinute();
    }
}