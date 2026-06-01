using API.Entities;

namespace API.Authentication.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
