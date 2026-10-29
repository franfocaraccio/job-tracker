namespace JobTracker.Api.Contracts;

public sealed record RegisterRequest(string Name, string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string AccessToken);

public sealed record UserResponse(Guid Id, string Name, string Email);
