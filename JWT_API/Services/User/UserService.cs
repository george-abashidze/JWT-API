using JWT_API.Data;
using JWT_API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JWT_API.Services.User
{
    public class UserService: IUserService
    {
        ApplicationDbContext _db;
        public UserService(ApplicationDbContext db) {
            _db = db;
        }

        public async Task<UserEntity?> GetById(int id) {
            return await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
