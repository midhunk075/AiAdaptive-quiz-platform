using API.Configuration;
using API.Entities;
using API.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services;

public class TokenService(IOptions<JwtOptions> jwtOptions) : ITokenService
{
    public string CreateToken(AppUser user)
    {
        var options = jwtOptions.Value;
        var tokenKey = options.Key ?? throw new InvalidOperationException("Cannot access token key");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role)
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(options.ExpiryDays),
            SigningCredentials = creds
        };

        if (!string.IsNullOrWhiteSpace(options.Issuer))
        {
            tokenDescriptor.Issuer = options.Issuer;
        }

        if (!string.IsNullOrWhiteSpace(options.Audience))
        {
            tokenDescriptor.Audience = options.Audience;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
