using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Clean.Permissions;

using Microsoft.AspNetCore.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; private set; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}