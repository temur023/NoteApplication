using System.Security.Claims;
using Clean.Application.Abstractions;
using Clean.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;
    private readonly IDbContext _context;

    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger, IDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Check whether a given PermissionRequirement is satisfied or not for a particular context
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        _logger.LogWarning("Evaluating authorization requirement for permission {permission}", requirement.Permission);
        var user = context.User;   
        var userId = user.Claims.FirstOrDefault(x=>x.Type == ClaimTypes.NameIdentifier)?.Value;
        if(userId == null)
            return Task.CompletedTask;

        foreach (Claim claim in context.User.Claims)
        {
            if (claim.Type != "Permission" || claim.Value != requirement.Permission)
                continue;
            
            if (claim.Type == "Permission" && claim.Value == requirement.Permission)
            {
                _logger.LogInformation("Permission {permission} is satisfied", requirement.Permission);
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}

