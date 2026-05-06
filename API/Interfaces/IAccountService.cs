using API.DTO;

namespace API.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<UserDTO>> GetUsersAsync();
    Task<UserDTO> GetUserAsync(string id);
    Task<UserDTO> RegisterAsync(RegisterDTO registerDto);
    Task<UserDTO> LoginAsync(LoginDTO loginDto);
}
