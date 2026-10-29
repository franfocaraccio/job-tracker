using JobTracker.Application.Users;
using Microsoft.AspNetCore.Identity;

namespace JobTracker.Infrastructure.Authentication;

/// <summary>
/// Hashes passwords with the .NET Identity hasher.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();

    public string Hash(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        var result = _hasher.VerifyHashedPassword(null!, passwordHash, password);

        return result != PasswordVerificationResult.Failed;
    }
}
