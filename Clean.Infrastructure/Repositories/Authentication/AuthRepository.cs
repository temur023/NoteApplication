using Clean.Application.Abstractions;
using Clean.Application.Dtos.User;
using Clean.Application.Responses;
using Clean.Application.Services;
using Clean.Domain.Entities;
using Clean.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clean.Infrastructure.Repositories.Authentication;

public class AuthRepository(DataContext context, ITokenRepository service):IAuthRepository
{
    public async Task<Response<LoginResponseDto>> Login(LoginRequestDto dto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u=>u.Name == dto.Name);
        if(user == null) return new Response<LoginResponseDto>(404,"User not found");
        
        if (!BCrypt.Net.BCrypt.Verify(dto.UserPassword, user.PasswordHash))
            return new Response<LoginResponseDto>(401, "Invalid user password!");

        var token = await service.GenerateJwtToken(user);

        var response = new LoginResponseDto
        {
            Token = token,
            Name = user.Name,
            Role = user.Role.ToString()
        };
        return new Response<LoginResponseDto>(200,"Login successful", response);
    }

    public async Task<Response<UserGetDto>> Create(UserCreateDto dto)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);
        
        var model = new User()
        {
            Name = dto.Name,
            PasswordHash = hashedPassword,
            Role = dto.Role
        };
        context.Users.Add(model);
        await context.SaveChangesAsync();
        var userDto = new UserGetDto()
        {
            Id = model.Id,
            Name = dto.Name,
            PassworodHash = dto.PasswordHash,
            Role = model.Role
        };
        return new Response<UserGetDto>(200, "User created", userDto);
    }
}