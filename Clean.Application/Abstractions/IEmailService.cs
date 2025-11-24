namespace Clean.Application.Abstractions;

public interface IEmailService
{
    Task SendEmailEveryMinute();
}