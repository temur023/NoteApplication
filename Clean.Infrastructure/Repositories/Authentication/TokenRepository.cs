using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clean.Application.Services;
using Clean.Domain.Entities;
using Clean.Permissions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Clean.Infrastructure.Repositories.Authentication;

public class TokenRepository(IConfiguration configuration):ITokenService
{
    public Task<string> GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        if (RolePermissionMapping.RolePermissions.TryGetValue(user.Role, out var permissions))
        {
            foreach (var permission in permissions)
                claims.Add(new Claim("Permission", permission));
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
            );
        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(jwtToken);
    }
}