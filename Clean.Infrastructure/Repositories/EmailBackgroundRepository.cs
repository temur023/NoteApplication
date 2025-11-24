using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Clean.Application.Abstractions;

namespace Clean.Infrastructure.Repositories;



public class EmailBackgroundService(IEmailRepository repository) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await repository.SendEmailEveryMinute();

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
