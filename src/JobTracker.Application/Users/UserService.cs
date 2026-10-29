using System.ComponentModel.DataAnnotations;
using JobTracker.Domain.Users;

namespace JobTracker.Application.Users;

/// <summary>
/// Use cases for registering users, signing them in and reading their account.
/// </summary>
public sealed class UserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public UserService(IUserRepository repository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<User> RegisterAsync(string name, string email, string password, CancellationToken cancellationToken)
    {
        User.ValidatePassword(password);

        var user = User.Create(name, email, _passwordHasher.Hash(password));

        var existingUser = await _repository.GetByEmailAsync(user.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new ValidationException("Email is already registered.");
        }

        await _repository.AddAsync(user, cancellationToken);

        return user;
    }

    public async Task<string?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByEmailAsync(email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        return _tokenService.CreateToken(user);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }
}
