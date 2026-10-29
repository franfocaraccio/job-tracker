using System.Security.Claims;
using System.Text;
using JobTracker.Application.Users;
using JobTracker.Domain.Users;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace JobTracker.Infrastructure.Authentication;

/// <summary>
/// Issues signed JWT access tokens that identify the signed-in user.
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public TokenService(JwtSettings settings)
    {
        _settings = settings;
    }

    public string CreateToken(User user)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
            ]),
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
