namespace JobTracker.Infrastructure.Authentication;

/// <summary>
/// JWT configuration shared by the API that issues tokens and the API that validates them.
/// </summary>
public sealed class JwtSettings
{
    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public required string SigningKey { get; init; }

    public int ExpirationMinutes { get; init; } = 60;
}
