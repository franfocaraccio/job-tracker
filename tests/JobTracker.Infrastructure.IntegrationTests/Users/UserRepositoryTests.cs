using JobTracker.Domain.Users;
using JobTracker.Infrastructure.Users;

namespace JobTracker.Infrastructure.IntegrationTests.Users;

public class UserRepositoryTests : IClassFixture<PostgresFixture>
{
    private readonly UserRepository _repository;

    public UserRepositoryTests(PostgresFixture fixture)
    {
        _repository = new UserRepository(fixture.DataSource);
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ReturnsTheSavedUser()
    {
        var user = NewUser();

        await _repository.AddAsync(user, CancellationToken.None);
        var result = await _repository.GetByIdAsync(user.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.PasswordHash, result.PasswordHash);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ReturnsTheUser()
    {
        var user = NewUser();
        await _repository.AddAsync(user, CancellationToken.None);

        var result = await _repository.GetByEmailAsync(user.Email, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var result = await _repository.GetByEmailAsync("notexist@jobtracker.com", CancellationToken.None);

        Assert.Null(result);
    }

    private static User NewUser()
    {
        return User.Create("Demo User", $"{Guid.NewGuid()}@jobtracker.com", "hashed-password");
    }
}
