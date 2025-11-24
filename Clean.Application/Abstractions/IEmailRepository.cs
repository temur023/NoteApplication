namespace Clean.Application.Abstractions;

public interface IEmailRepository
{
    Task SendEmailEveryMinute();
}