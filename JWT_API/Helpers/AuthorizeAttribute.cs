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

            //if role required
            if (_roleName != null) {

                var hasClaim = user.Roles.Any(r => r.Name.Equals(_roleName));

                if (!hasClaim)
                {
                    context.Result = new NotFoundResult();
                }
            }
            
        }
    }
}
