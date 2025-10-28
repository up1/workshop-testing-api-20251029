using api.Models;

namespace api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneAsync(string phone);
    Task<User> CreateAsync(User user);
    Task<User?> GetByIdAsync(string id);
}
