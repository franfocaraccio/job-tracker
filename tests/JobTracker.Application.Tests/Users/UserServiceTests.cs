using System.ComponentModel.DataAnnotations;
using JobTracker.Application.Users;
using JobTracker.Domain.Users;
using NSubstitute;

namespace JobTracker.Application.Tests.Users;

public class UserServiceTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _passwordHasher.Hash("secret123").Returns("hashed-secret123");
        _passwordHasher.Verify("secret123", "hashed-secret123").Returns(true);

        _service = new UserService(_repository, _passwordHasher, _tokenService);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_SavesUserWithHashedPassword()
    {
        var user = await _service.RegisterAsync("Demo User", "demo@jobtracker.com", "secret123", CancellationToken.None);

        Assert.Equal("Demo User", user.Name);
        Assert.Equal("demo@jobtracker.com", user.Email);
        Assert.Equal("hashed-secret123", user.PasswordHash);
        await _repository.Received(1).AddAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_WithShortPassword_ThrowsAndDoesNotSave()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.RegisterAsync("Demo User", "demo@jobtracker.com", "123", CancellationToken.None));

        await _repository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_WithEmailAlreadyRegistered_ThrowsAndDoesNotSave()
    {
        var existingUser = User.Create("Someone Else", "demo@jobtracker.com", "hashed-other");
        _repository.GetByEmailAsync("demo@jobtracker.com", Arg.Any<CancellationToken>()).Returns(existingUser);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.RegisterAsync("Demo User", "Demo@JobTracker.com", "secret123", CancellationToken.None));

        Assert.Equal("Email is already registered.", exception.Message);
        await _repository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = User.Create("Demo User", "demo@jobtracker.com", "hashed-secret123");
        _repository.GetByEmailAsync("demo@jobtracker.com", Arg.Any<CancellationToken>()).Returns(user);
        _tokenService.CreateToken(user).Returns("jwt-token");

        var token = await _service.LoginAsync(" Demo@JobTracker.com ", "secret123", CancellationToken.None);

        Assert.Equal("jwt-token", token);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ReturnsNull()
    {
        var token = await _service.LoginAsync("unknown@jobtracker.com", "secret123", CancellationToken.None);

        Assert.Null(token);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsNull()
    {
        var user = User.Create("Demo User", "demo@jobtracker.com", "hashed-secret123");
        _repository.GetByEmailAsync("demo@jobtracker.com", Arg.Any<CancellationToken>()).Returns(user);

        var token = await _service.LoginAsync("demo@jobtracker.com", "wrong-password", CancellationToken.None);

        Assert.Null(token);
        _tokenService.DidNotReceive().CreateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsTheUser()
    {
        var user = User.Create("Demo User", "demo@jobtracker.com", "hashed-secret123");
        _repository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _service.GetByIdAsync(user.Id, CancellationToken.None);

        Assert.Same(user, result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }
}
