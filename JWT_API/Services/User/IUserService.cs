using JWT_API.Data.Entities;

namespace JWT_API.Services.User
{
    public interface IUserService
    {
        Task<UserEntity?> GetById(int id);
    }
}
