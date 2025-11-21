using Clean.Application.Abstractions;
using Clean.Application.Dtos.User;
using Clean.Application.Filters;
using Clean.Application.Responses;
using Clean.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clean.Infrastructure.Repositories;

public class UserRepository(DataContext context):IUserRepository
{
    public async Task<PagedResponse<UserGetDto>> GetAll(UserFilter filter)
    {
        var query = context.Users.AsQueryable();
        if(!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(u=>u.Name.ToLower().Contains(filter.Name.ToLower()));
        if(!String.IsNullOrWhiteSpace(filter.Role))
            query = query.Where(u=>u.Role.ToString().ToLower() == filter.Role.ToLower());
        var totalRecors = await query.CountAsync();
        var users = await query.Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize).ToListAsync();
        var dto = users.Select(u => new UserGetDto()
        {
            Id = u.Id,
            Name = u.Name,
            PassworodHash = u.PasswordHash,
            Role = u.Role
        }).ToList();
        return new PagedResponse<UserGetDto>
            (dto,
            filter.PageNumber, 
            filter.PageSize,
            totalRecors);
    }

        public async Task<Response<UserGetDto>> GetById(int id)
        {
            var find = await context.Users.FindAsync(id);
            if(find == null) return new Response<UserGetDto>(404,"User not found");
            var dto = new UserGetDto()
            {
                Id = find.Id,
                Name = find.Name,
                PassworodHash = find.PasswordHash,
                Role = find.Role
            };
            return new Response<UserGetDto>(200,"User Found!", dto);
        }

    public async Task<Response<UserGetDto>> Update(UserUpdateDto dto)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);
        var find = await context.Users.FindAsync(dto.Id);
        if(find == null) return new Response<UserGetDto>(404,"User not found");
        find.Name = dto.Name;
        find.PasswordHash = hashedPassword;
        find.Role = dto.Role;
        await context.SaveChangesAsync();
        var user = new UserGetDto()
        {
            Id = find.Id,
            Name = find.Name,
            PassworodHash = find.PasswordHash,
            Role = find.Role
        };
        return new Response<UserGetDto>(200,"User Updated!", user);
    }

    public async Task<Response<string>> Delete(int id)
    {
        var find = await context.Users.FindAsync(id);
        if(find == null) return new Response<string>(404,"User not found");
        context.Users.Remove(find);
        await context.SaveChangesAsync();
        return new Response<string>(200,"User Deleted!");
    }
}