using API.Entities;

namespace API.Interfaces;

public interface IUserRepository
{
    Task<bool> UserExistsAsync(string userName);
    Task<AppUser?> GetByUsernameAsync(string userName);
    Task<AppUser?> GetByIdAsync(string id);
    Task<IEnumerable<AppUser>> GetAllAsync();
    Task AddAsync(AppUser user);
    Task SaveChangesAsync();
}
