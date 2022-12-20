using JWT_API.Services;
using JWT_API.Services.User;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace JWT_API.Helpers
{
    public class JWTMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IJwtAuthManager _jwtAuthManager;

        public JWTMiddleware(RequestDelegate next, IJwtAuthManager jwtAuthManager)
        {
            _next = next;
            _jwtAuthManager = jwtAuthManager;
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                await AttachUserToContext(context, userService, token);

            await _next(context);
        }

        private async Task AttachUserToContext(HttpContext context, IUserService userService, string accessToken)
        {
            try
            {
                var (claims,jwtToken) = _jwtAuthManager.DecodeJwtToken(accessToken);

                if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                if (jwtToken.ValidTo < DateTime.Now) {
                    throw new SecurityTokenException("Out dated token");
                }

                var userId = int.Parse(claims.Claims.First(x => x.Type.Equals("id")).Value);

                // attach user to context on successful jwt validation
                context.Items["User"] = await userService.GetById(userId);
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}

