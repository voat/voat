namespace Voat.Models
{
    public enum Permissions
    {
        Admin = 1,
        Moderator = 2,
    }

    public static class PermissionExtensions
    {
        public static bool IsElevatedAccess(this Permissions? level)
        {
            return level == Permissions.Admin || level == Permissions.Moderator;
        }
    }
}