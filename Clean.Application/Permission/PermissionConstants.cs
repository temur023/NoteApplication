namespace Clean.Permissions;

public static class PermissionConstants
{
    
    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string ManageSelf = "Permissions.Users.ManageSelf";
        public const string Manage = "Permissions.Users.Manage";
    }

    public static class Notes
    {
        public const string View = "Permissions.Notes.View";
        public const string Manage = "Permissions.Notes.Manage";
    }

    public static class Reminders
    {
        public const string View = "Permissions.Reminders.View";
        public const string Manage = "Permissions.Reminders.Manage";
    }
    
}