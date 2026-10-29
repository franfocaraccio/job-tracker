using JobTracker.Domain.JobApplications;
using JobTracker.Domain.Users;
using JobTracker.Infrastructure.JobApplications;
using JobTracker.Infrastructure.Users;

namespace JobTracker.Infrastructure.IntegrationTests.JobApplications;

public class JobApplicationRepositoryTests : IClassFixture<PostgresFixture>
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly JobApplicationRepository _repository;
    private readonly UserRepository _userRepository;

    public JobApplicationRepositoryTests(PostgresFixture fixture)
    {
        _repository = new JobApplicationRepository(fixture.DataSource);
        _userRepository = new UserRepository(fixture.DataSource);
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ReturnsTheSavedApplication()
    {
        var user = await AddUserAsync();
        var application = JobApplication.Create(user.Id, "Ballast Lane", ".NET Developer", Today);

        await _repository.AddAsync(application, CancellationToken.None);
        var result = await _repository.GetByIdAsync(application.Id, user.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(application.Id, result.Id);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("Ballast Lane", result.Company);
        Assert.Equal(".NET Developer", result.Position);
        Assert.Equal(ApplicationStatus.Applied, result.Status);
        Assert.Equal(Today, result.AppliedOn);
    }

    [Fact]
    public async Task GetByIdAsync_WithAnotherUser_ReturnsNull()
    {
        var owner = await AddUserAsync();
        var anotherUser = await AddUserAsync();
        var application = JobApplication.Create(owner.Id, "Ballast Lane", ".NET Developer", Today);
        await _repository.AddAsync(application, CancellationToken.None);

        var result = await _repository.GetByIdAsync(application.Id, anotherUser.Id, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyTheUserApplicationsNewestFirst()
    {
        var user = await AddUserAsync();
        var anotherUser = await AddUserAsync();
        var older = JobApplication.Create(user.Id, "Google", "AI Engineer", Today.AddDays(-10));
        var newer = JobApplication.Create(user.Id, "Ballast Lane", ".NET Developer", Today);
        var notMine = JobApplication.Create(anotherUser.Id, "OpenAI", "Full Stack Developer", Today);
        await _repository.AddAsync(older, CancellationToken.None);
        await _repository.AddAsync(newer, CancellationToken.None);
        await _repository.AddAsync(notMine, CancellationToken.None);

        var result = await _repository.GetByUserAsync(user.Id, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(newer.Id, result[0].Id);
        Assert.Equal(older.Id, result[1].Id);
    }

    [Fact]
    public async Task UpdateAsync_SavesTheChanges()
    {
        var user = await AddUserAsync();
        var application = JobApplication.Create(user.Id, "Ballast Lane", ".NET Developer", Today);
        await _repository.AddAsync(application, CancellationToken.None);

        application.UpdateDetails("Google", "Senior .NET Developer", Today.AddDays(-1));
        application.ChangeStatus(ApplicationStatus.Interviewing);
        await _repository.UpdateAsync(application, CancellationToken.None);
        var result = await _repository.GetByIdAsync(application.Id, user.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Google", result.Company);
        Assert.Equal("Senior .NET Developer", result.Position);
        Assert.Equal(ApplicationStatus.Interviewing, result.Status);
        Assert.Equal(Today.AddDays(-1), result.AppliedOn);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheApplication()
    {
        var user = await AddUserAsync();
        var application = JobApplication.Create(user.Id, "Ballast Lane", ".NET Developer", Today);
        await _repository.AddAsync(application, CancellationToken.None);

        await _repository.DeleteAsync(application.Id, CancellationToken.None);
        var result = await _repository.GetByIdAsync(application.Id, user.Id, CancellationToken.None);

        Assert.Null(result);
    }

    private async Task<User> AddUserAsync()
    {
        var user = User.Create("Demo User", $"{Guid.NewGuid()}@jobtracker.com", "hashed-password");
        await _userRepository.AddAsync(user, CancellationToken.None);
        return user;
    }
}
