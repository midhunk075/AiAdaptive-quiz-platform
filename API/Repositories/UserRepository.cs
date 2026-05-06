using API.Data;
using API.Entities;
using API.Exceptions;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<bool> UserExistsAsync(string userName)
    {
        var normalizedUserName = userName.ToLowerInvariant();
        return await context.Users.AnyAsync(x => x.UserName == normalizedUserName);
    }

    public async Task<AppUser?> GetByUsernameAsync(string userName)
    {
        var normalizedUserName = userName.ToLowerInvariant();
        return await context.Users.SingleOrDefaultAsync(x => x.UserName == normalizedUserName);
    }

    public async Task<AppUser?> GetByIdAsync(string id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync()
    {
        return await context.Users.ToListAsync();
    }

    public async Task AddAsync(AppUser user)
    {
        await context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Database update conflict.");
        }
    }

}
