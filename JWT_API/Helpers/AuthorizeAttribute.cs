using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using JWT_API.Data.Entities;
using System.Security.Claims;
using JWT_API.Enums;

namespace JWT_API.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        readonly string? _roleName;

        public AuthorizeAttribute(string roleName)
        {
            _roleName= roleName;
        }

        public AuthorizeAttribute()
        {
           
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {

            var user = (UserEntity)context.HttpContext.Items["User"];
            if (user == null)
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
                return;
            }

            //check hierarchy of roles
            //for example admin must see User content also
            //or power user must see both user and admin content also
            if (_roleName != null) {

                var hasClaim = user.Roles.Any(r => r.Name.Equals(_roleName));

                if (!hasClaim && _roleName.Equals(Enum.GetName(typeof(Role), Role.User)))
                {
                    hasClaim = user.Roles.Any(r => r.Name.Equals(Enum.GetName(typeof(Role), Role.Admin)) || r.Name.Equals(Enum.GetName(typeof(Role), Role.PowerUser)));
                }
                else if (!hasClaim && _roleName.Equals(Enum.GetName(typeof(Role), Role.Admin)))
                {
                    hasClaim = user.Roles.Any(r => r.Name.Equals(Enum.GetName(typeof(Role), Role.PowerUser)));
                }

                if (!hasClaim)
                {
                    context.Result = new NotFoundResult();
                }
            }
            
        }
    }
}
