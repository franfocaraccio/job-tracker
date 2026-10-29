using System.ComponentModel.DataAnnotations;

namespace JobTracker.Domain.Users;

/// <summary>
/// A registered account that owns job applications.
/// </summary>
public sealed class User
{
    public const int MaxNameLength = 100;
    public const int MinPasswordLength = 6;

    private User(Guid id, string name, string email, string passwordHash)
    {
        Id = id;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Email { get; }

    public string PasswordHash { get; }

    public static User Create(string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Name is required.");
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw new ValidationException($"Name must be {MaxNameLength} characters or fewer.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException("Email is required.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (!new EmailAddressAttribute().IsValid(normalizedEmail))
        {
            throw new ValidationException("Email is not a valid email address.");
        }

        return new User(Guid.NewGuid(), name.Trim(), normalizedEmail, passwordHash);
    }

    public static void ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
        {
            throw new ValidationException($"Password must be at least {MinPasswordLength} characters long.");
        }
    }
}
