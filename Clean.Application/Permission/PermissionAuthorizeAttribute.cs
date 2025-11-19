using Microsoft.AspNetCore.Authorization;

namespace Clean.Permissions;


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        public PermissionAuthorizeAttribute(string permission)
        {
            AuthenticationSchemes = "Bearer";
            Policy = permission;
        }
        
    }