using JWT_API.Data;
using JWT_API.Data.Entities;
using JWT_API.Enums;
using JWT_API.Helpers;
using JWT_API.Models.Request;
using Microsoft.AspNetCore.Http;
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
        SignUpConfig _signUpConfig;
        IHttpContextAccessor _httpContextAccessor;

        public AuthController(ApplicationDbContext db, IJwtAuthManager jwtManager,JwtTokenConfig jwtConfig, SignUpConfig signUpConfig, IHttpContextAccessor httpContextAccessor) { 
            _db= db;
            _jwtManager= jwtManager;
            _jwtConfig= jwtConfig;
            _signUpConfig= signUpConfig;
            _httpContextAccessor= httpContextAccessor;
        }

        [HttpPost]
        [Route("/Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request) {

            var user = await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u =>
            u.Email.Equals(request.Email.Trim().ToLower()) &&
            u.PasswordHash.Equals(HashUtil.GetStringHash(request.Password,_jwtConfig.Secret)));

            if (user == null)
                return NotFound();

            if (_signUpConfig.RequireAccountComfirmation && !user.AccountConfirmed) {
                return Ok("Account cofirmation required.");
            }

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

        [HttpGet]
        [Route("/GetUser")]
        [Authorize]
        public async Task<IActionResult> GetUser() {

            try
            {
                var user = (UserEntity)_httpContextAccessor.HttpContext.Items["User"];

                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch(Exception ex) {

                return NotFound();
            }
            
        }


        [HttpPost]
        [Route("/SignUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {

            var validationResult = request.Validate(_signUpConfig);
            if (validationResult != null)
                return BadRequest(validationResult);

            var emailRegistered = await _db.Users.AnyAsync(u =>
            u.Email.Equals(request.Email.Trim().ToLower()));

            if (emailRegistered)
                return BadRequest("Email already registered.");

            try
            {
                UserEntity user = new();
                user.Email = request.Email.Trim().ToLower();
                user.FirstName = request.FirstName.Trim();
                user.LastName = request.LastName.Trim();
                user.PasswordHash = HashUtil.GetStringHash(request.Password, _jwtConfig.Secret);
                user.PhoneNumber = request.PhoneNumber;

                user.Roles = new List<RoleEntity>();

                var userRole = await _db.Roles.FirstAsync(r => r.Name.Equals(Enum.GetName(typeof(Role), Role.User)));
                user.Roles.Add(userRole);
                _db.Users.Add(user);
                await _db.SaveChangesAsync();

            }
            catch(Exception ex)
            {
                return BadRequest("Error");
            }

            if (_signUpConfig.RequireAccountComfirmation) {
                //TODO: send email confirmation code
                return Ok("Confirmation code sent.");
            }
                

            return Ok("Success");

        }
    }
}
