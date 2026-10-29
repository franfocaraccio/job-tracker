using System.ComponentModel.DataAnnotations;
using JobTracker.Application.JobApplications;
using JobTracker.Domain.JobApplications;
using NSubstitute;

namespace JobTracker.Application.Tests.JobApplications;

public class JobApplicationServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly IJobApplicationRepository _repository = Substitute.For<IJobApplicationRepository>();
    private readonly JobApplicationService _service;

    public JobApplicationServiceTests()
    {
        _service = new JobApplicationService(_repository);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTheUserApplications()
    {
        var applications = new List<JobApplication> { NewApplication(), NewApplication() };
        _repository.GetByUserAsync(UserId, Arg.Any<CancellationToken>()).Returns(applications);

        var result = await _service.GetAllAsync(UserId, CancellationToken.None);

        Assert.Equal(applications, result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsTheApplication()
    {
        var application = NewApplication();
        _repository.GetByIdAsync(application.Id, UserId, Arg.Any<CancellationToken>()).Returns(application);

        var result = await _service.GetByIdAsync(application.Id, UserId, CancellationToken.None);

        Assert.Same(application, result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid(), UserId, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_SavesAndReturnsTheApplication()
    {
        var result = await _service.CreateAsync(UserId, "Ballast Lane", ".NET Developer", Today, CancellationToken.None);

        Assert.Equal(UserId, result.UserId);
        Assert.Equal("Ballast Lane", result.Company);
        Assert.Equal(ApplicationStatus.Applied, result.Status);
        await _repository.Received(1).AddAsync(result, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithFutureDate_ThrowsAndDoesNotSave()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(UserId, "Ballast Lane", ".NET Developer", Today.AddDays(1), CancellationToken.None));

        await _repository.DidNotReceive().AddAsync(Arg.Any<JobApplication>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNullAndDoesNotSave()
    {
        var result = await _service.UpdateAsync(
            Guid.NewGuid(), UserId, "Google", ".NET Developer", Today, ApplicationStatus.Applied, CancellationToken.None);

        Assert.Null(result);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<JobApplication>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithNewDetailsAndStatus_SavesTheChanges()
    {
        var application = NewApplication();
        _repository.GetByIdAsync(application.Id, UserId, Arg.Any<CancellationToken>()).Returns(application);

        var result = await _service.UpdateAsync(
            application.Id, UserId, "Google", "Senior .NET Developer", Today, ApplicationStatus.Interviewing, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Google", result.Company);
        Assert.Equal("Senior .NET Developer", result.Position);
        Assert.Equal(ApplicationStatus.Interviewing, result.Status);
        await _repository.Received(1).UpdateAsync(application, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithSameStatus_UpdatesOnlyTheDetails()
    {
        var application = NewApplication();
        _repository.GetByIdAsync(application.Id, UserId, Arg.Any<CancellationToken>()).Returns(application);

        var result = await _service.UpdateAsync(
            application.Id, UserId, "Google", ".NET Developer", Today, ApplicationStatus.Applied, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Google", result.Company);
        Assert.Equal(ApplicationStatus.Applied, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidStatusTransition_ThrowsAndDoesNotSave()
    {
        var application = NewApplication();
        _repository.GetByIdAsync(application.Id, UserId, Arg.Any<CancellationToken>()).Returns(application);

        await Assert.ThrowsAsync<ValidationException>(
            () => _service.UpdateAsync(
                application.Id, UserId, "Ballast Lane", ".NET Developer", Today, ApplicationStatus.Accepted, CancellationToken.None));

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<JobApplication>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_DeletesAndReturnsTrue()
    {
        var application = NewApplication();
        _repository.GetByIdAsync(application.Id, UserId, Arg.Any<CancellationToken>()).Returns(application);

        var result = await _service.DeleteAsync(application.Id, UserId, CancellationToken.None);

        Assert.True(result);
        await _repository.Received(1).DeleteAsync(application.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalseAndDoesNotDelete()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid(), UserId, CancellationToken.None);

        Assert.False(result);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private static JobApplication NewApplication()
    {
        return JobApplication.Create(UserId, "Ballast Lane", ".NET Developer", Today);
    }
}
