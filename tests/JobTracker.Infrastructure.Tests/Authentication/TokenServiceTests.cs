using System.Text;
using JobTracker.Domain.Users;
using JobTracker.Infrastructure.Authentication;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace JobTracker.Infrastructure.Tests.Authentication;

public class TokenServiceTests
{
    private static readonly JwtSettings Settings = new JwtSettings
    {
        Issuer = "JobTracker.Identity",
        Audience = "JobTracker",
        SigningKey = "test-signing-key-with-at-least-32-characters",
        ExpirationMinutes = 60,
    };

    [Fact]
    public async Task CreateToken_ReturnsSignedTokenWithTheUserClaims()
    {
        var user = User.Create("Demo User", "demo@jobtracker.com", "hashed-password");

        var token = new TokenService(Settings).CreateToken(user);
        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, ValidationParameters(Settings.SigningKey));

        Assert.True(result.IsValid);
        Assert.Equal(user.Id.ToString(), result.Claims[JwtRegisteredClaimNames.Sub]);
        Assert.Equal("demo@jobtracker.com", result.Claims[JwtRegisteredClaimNames.Email]);
        Assert.Equal("Demo User", result.Claims[JwtRegisteredClaimNames.Name]);
    }

    [Fact]
    public async Task CreateToken_IsRejectedWhenValidatedWithAnotherKey()
    {
        var user = User.Create("Demo User", "demo@jobtracker.com", "hashed-password");

        var token = new TokenService(Settings).CreateToken(user);
        var result = await new JsonWebTokenHandler().ValidateTokenAsync(
            token, ValidationParameters("another-signing-key-with-at-least-32-chars"));

        Assert.False(result.IsValid);
    }

    private static TokenValidationParameters ValidationParameters(string signingKey)
    {
        return new TokenValidationParameters
        {
            ValidIssuer = Settings.Issuer,
            ValidAudience = Settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
        };
    }
}
