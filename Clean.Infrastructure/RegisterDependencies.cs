using Clean.Application.Abstractions;
using Clean.Application.Services;
using Clean.Infrastructure.Data;
using Clean.Infrastructure.Repositories;
using Clean.Infrastructure.Repositories.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Clean.Infrastructure;

public static class RegisterDependencies
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IDbContext>(provider => provider.GetRequiredService<DataContext>());
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IReminderRepository, ReminderRepository>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<HttpContextAccessor>();
        return services;
    }
    
}