using Clean.Domain.Entities;
using Clean.Permissions;

namespace Clean.Permissions;

public static class RolePermissionMapping
{
    public static readonly Dictionary<Role, List<string>> RolePermissions = new()
    {
        {
            Role.Admin, new List<string>
            {
            }
        },
        {
            Role.User, new List<string>
            {
            }
        },
    };
}