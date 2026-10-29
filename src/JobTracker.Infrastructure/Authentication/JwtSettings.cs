namespace JobTracker.Infrastructure.Authentication;

/// <summary>
/// JWT configuration used to issue tokens on login and to validate them on protected endpoints.
/// </summary>
public sealed class JwtSettings
{
    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public required string SigningKey { get; init; }

    public int ExpirationMinutes { get; init; } = 60;
}
