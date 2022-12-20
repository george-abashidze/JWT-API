using JWT_API.Data;
using JWT_API.Data.Entities;
using JWT_API.Helpers;
using JWT_API.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JWT_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        ApplicationDbContext _db;
        IJwtAuthManager _jwtManager;
        JwtTokenConfig _jwtConfig;


        public AuthController(ApplicationDbContext db, IJwtAuthManager jwtManager,JwtTokenConfig jwtConfig) { 
            _db= db;
            _jwtManager= jwtManager;
            _jwtConfig= jwtConfig;
        }

        [HttpPost]
        [Route("/Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request) {

            var user = await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u =>
            u.Email.Equals(request.Email.Trim().ToLower()) &&
            u.PasswordHash.Equals(HashUtil.GetStringHash(request.Password,_jwtConfig.Secret)));

            if (user == null)
                return NotFound();

            ClaimsIdentity claimsIdentity = new();

            foreach (RoleEntity role in user.Roles) {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Name));
            }

            claimsIdentity.AddClaim(new Claim("id",user.Id.ToString()));

            return Ok(new {
                User = user,
                Token = _jwtManager.GenerateToken(claimsIdentity.Claims.ToArray(), DateTime.Now)
            });

        }
    }
}
