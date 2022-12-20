using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JWT_API.Data.Entities;
using JWT_API.Enums;

namespace JWT_API.Helpers
{
    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        readonly Claim _claim;

        public ClaimRequirementFilter(Claim claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = (UserEntity)context.HttpContext.Items["User"];

            if (user == null) {

                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
                return;
            }
           
            //check hierarchy of roles
            //for example admin must see User content also
            //or power user must see both user and admin content also

            var hasClaim = user.Roles.Any(r => r.Name.Equals(_claim.Value));

            if (!hasClaim && _claim.Value.Equals(Enum.GetName(typeof(Role), Role.User)))
            {
                hasClaim = user.Roles.Any(r => r.Name.Equals(Enum.GetName(typeof(Role), Role.Admin)) || r.Name.Equals(Enum.GetName(typeof(Role), Role.PowerUser)));
            }
            else if (!hasClaim && _claim.Value.Equals(Enum.GetName(typeof(Role), Role.Admin))) {
                hasClaim = user.Roles.Any(r => r.Name.Equals(Enum.GetName(typeof(Role), Role.PowerUser)));
            }

            if (!hasClaim)
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}
