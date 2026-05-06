using API.DTO;
using API.Entities;
using API.Interfaces;
using Microsoft.Identity.Client;
using API.Exceptions;

namespace API.Services;

public class AccountService(
    IUserRepository userRepository,
    IPasswordService passwordService,
    ITokenService tokenService) : IAccountService
{
    public async Task<IEnumerable<UserDTO>> GetUsersAsync()
    {
        var users = await userRepository.GetAllAsync();
        return users.Select(user => MapUser(user));
    }

    public async Task<UserDTO> GetUserAsync(string id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null) { 
            throw new NotFoundException("User not found!");
        }
        return MapUser(user);
    }

    public async Task<UserDTO> RegisterAsync(RegisterDTO registerDto)
    {
        var normalizedUserName = registerDto.UserName.ToLowerInvariant();
        if (await userRepository.UserExistsAsync(normalizedUserName))
        {
            throw new ConflictException("Username is already taken!");
        }

        passwordService.CreatePasswordHash(registerDto.Password, out var passwordHash, out var passwordSalt);

        var user = new AppUser
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            UserName = normalizedUserName,
            EmailAddress = registerDto.EmailAddress,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Role = registerDto.Role
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        return MapUser(user, includeToken: true);
    }

    public async Task<UserDTO> LoginAsync(LoginDTO loginDto)
    {
        var user = await userRepository.GetByUsernameAsync(loginDto.UserName);
        if (user is null)
        {
            throw new ValidationException("Invalid credentials");
        }

        if (!passwordService.VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new ValidationException("Invalid Credentials!");
        }

        return MapUser(user, includeToken: true);
    }

    private UserDTO MapUser(AppUser user, bool includeToken = false)
    {
        return new UserDTO
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName,
            EmailAddress = user.EmailAddress,
            Role = user.Role,
            Token = includeToken ? tokenService.CreateToken(user) : null
        };
    }
}
