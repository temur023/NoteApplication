namespace Clean.Permissions;

public static class PermissionConstants
{

    public static class Company
    {
        public const string View = "Permissions.Company.View";
        public const string Manage = "Permissions.Company.Manage";
    }

    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string ManageSelf = "Permissions.Users.ManageSelf";
        public const string Manage = "Permissions.Users.Manage";
    }

    public static class Rooms
    {
        public const string View = "Permissions.Rooms.View";
        public const string Manage = "Permissions.Rooms.Manage";
    }
    public static class RoomEquipments
    {
        public const string View = "Permissions.RoomEquipments.View";
        public const string Manage = "Permissions.RoomEquipments.Manage";
    }

    public static class Bookings
    {
        public const string View = "Permissions.Bookings.View";
        public const string Manage = "Permissions.Bookings.Manage";
        public const string ManageSelf = "Permissions.Bookings.ManageSelf";
    }
    
}