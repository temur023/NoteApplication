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
                PermissionConstants.Users.Manage,
                PermissionConstants.Users.View,
                PermissionConstants.Notes.Manage,
                PermissionConstants.Notes.View,
                PermissionConstants.Reminders.Manage,
                PermissionConstants.Reminders.View
            }
        },
        {
            Role.User, new List<string>
            {
                PermissionConstants.Notes.Manage,
                PermissionConstants.Notes.View,
                PermissionConstants.Reminders.Manage,
                PermissionConstants.Reminders.View,
            }
        },
    };
}