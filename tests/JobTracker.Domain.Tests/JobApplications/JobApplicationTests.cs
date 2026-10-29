using System.ComponentModel.DataAnnotations;
using JobTracker.Domain.JobApplications;

namespace JobTracker.Domain.Tests.JobApplications;

public class JobApplicationTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Create_WithValidData_StartsInAppliedStatus()
    {
        var application = JobApplication.Create(UserId, "Ballast Lane", ".NET Developer", Today);

        Assert.NotEqual(Guid.Empty, application.Id);
        Assert.Equal(UserId, application.UserId);
        Assert.Equal("Ballast Lane", application.Company);
        Assert.Equal(".NET Developer", application.Position);
        Assert.Equal(Today, application.AppliedOn);
        Assert.Equal(ApplicationStatus.Applied, application.Status);
    }

    [Fact]
    public void Create_TrimsCompanyAndPosition()
    {
        var application = JobApplication.Create(UserId, "  Ballast Lane ", " .NET Developer  ", Today);

        Assert.Equal("Ballast Lane", application.Company);
        Assert.Equal(".NET Developer", application.Position);
    }

    [Fact]
    public void Create_WithEmptyUserId_ThrowsValidationError()
    {
        var exception = Assert.Throws<ValidationException>(
            () => JobApplication.Create(Guid.Empty, "Ballast Lane", ".NET Developer", Today));

        Assert.Equal("An application must belong to a user.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankCompany_ThrowsValidationError(string company)
    {
        var exception = Assert.Throws<ValidationException>(
            () => JobApplication.Create(UserId, company, ".NET Developer", Today));

        Assert.Equal("Company is required.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankPosition_ThrowsValidationError(string position)
    {
        var exception = Assert.Throws<ValidationException>(
            () => JobApplication.Create(UserId, "Ballast Lane", position, Today));

        Assert.Equal("Position is required.", exception.Message);
    }

    [Fact]
    public void Create_WithCompanyLongerThanMaxLength_ThrowsValidationError()
    {
        var company = new string('a', JobApplication.MaxTextLength + 1);

        var exception = Assert.Throws<ValidationException>(
            () => JobApplication.Create(UserId, company, ".NET Developer", Today));

        Assert.Equal("Company must be 100 characters or fewer.", exception.Message);
    }

    [Fact]
    public void Create_WithPositionLongerThanMaxLength_ThrowsValidationError()
    {
        var position = new string('a', JobApplication.MaxTextLength + 1);

        var exception = Assert.Throws<ValidationException>(
            () => JobApplication.Create(UserId, "Ballast Lane", position, Today));

        Assert.Equal("Position must be 100 characters or fewer.", exception.Message);
    }

    [Fact]
    public void Create_WithTextsAtMaxLength_Succeeds()
    {
        var text = new string('a', JobApplication.MaxTextLength);

        var application = JobApplication.Create(UserId, text, text, Today);

        Assert.Equal(text, application.Company);
        Assert.Equal(text, application.Position);
    }

    [Fact]
    public void Create_WithFutureAppliedOn_ThrowsValidationError()
    {
        var exception = Assert.Throws<ValidationException>(
            () => JobApplication.Create(UserId, "Ballast Lane", ".NET Developer", Today.AddDays(2)));

        Assert.Equal("The application date cannot be in the future.", exception.Message);
    }

    [Fact]
    public void Create_WithTomorrowInUtc_Succeeds()
    {
        var application = JobApplication.Create(UserId, "Ballast Lane", ".NET Developer", Today.AddDays(1));

        Assert.Equal(Today.AddDays(1), application.AppliedOn);
    }

    [Fact]
    public void Create_WithoutAppliedOn_ThrowsValidationError()
    {
        var exception = Assert.Throws<ValidationException>(
            () => JobApplication.Create(UserId, "Ballast Lane", ".NET Developer", default));

        Assert.Equal("The application date is required.", exception.Message);
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesFields()
    {
        var application = JobApplication.Create(UserId, "Ballast Lane", ".NET Developer", Today);

        application.UpdateDetails(" Google ", " Senior .NET Developer ", Today.AddDays(-3));

        Assert.Equal("Google", application.Company);
        Assert.Equal("Senior .NET Developer", application.Position);
        Assert.Equal(Today.AddDays(-3), application.AppliedOn);
    }

    [Fact]
    public void UpdateDetails_WithInvalidData_ThrowsAndKeepsOriginalValues()
    {
        var application = JobApplication.Create(UserId, "Ballast Lane", ".NET Developer", Today);

        Assert.Throws<ValidationException>(
            () => application.UpdateDetails("", ".NET Developer", Today));

        Assert.Equal("Ballast Lane", application.Company);
        Assert.Equal(".NET Developer", application.Position);
        Assert.Equal(Today, application.AppliedOn);
    }

    [Fact]
    public void ChangeStatus_ToAllowedStatus_UpdatesStatus()
    {
        var application = JobApplication.Create(UserId, "Ballast Lane", ".NET Developer", Today);

        application.ChangeStatus(ApplicationStatus.Interviewing);

        Assert.Equal(ApplicationStatus.Interviewing, application.Status);
    }

    [Fact]
    public void ChangeStatus_ToDisallowedStatus_ThrowsAndKeepsCurrentStatus()
    {
        var application = JobApplication.Create(UserId, "Ballast Lane", ".NET Developer", Today);

        var exception = Assert.Throws<ValidationException>(
            () => application.ChangeStatus(ApplicationStatus.Accepted));

        Assert.Equal("An application cannot move from 'Applied' to 'Accepted'.", exception.Message);
        Assert.Equal(ApplicationStatus.Applied, application.Status);
    }
}
