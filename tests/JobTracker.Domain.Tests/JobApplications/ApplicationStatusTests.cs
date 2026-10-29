using JobTracker.Domain.JobApplications;

namespace JobTracker.Domain.Tests.JobApplications;

public class ApplicationStatusTests
{
    [Theory]
    [InlineData(ApplicationStatus.Applied, ApplicationStatus.Interviewing)]
    [InlineData(ApplicationStatus.Applied, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Interviewing, ApplicationStatus.Offer)]
    [InlineData(ApplicationStatus.Interviewing, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Offer, ApplicationStatus.Accepted)]
    [InlineData(ApplicationStatus.Offer, ApplicationStatus.Rejected)]
    public void CanTransitionTo_NextStageOrRejection_ReturnsTrue(ApplicationStatus current, ApplicationStatus next)
    {
        Assert.True(current.CanTransitionTo(next));
    }

    [Theory]
    [InlineData(ApplicationStatus.Applied, ApplicationStatus.Offer)]
    [InlineData(ApplicationStatus.Applied, ApplicationStatus.Accepted)]
    [InlineData(ApplicationStatus.Interviewing, ApplicationStatus.Applied)]
    [InlineData(ApplicationStatus.Interviewing, ApplicationStatus.Accepted)]
    [InlineData(ApplicationStatus.Offer, ApplicationStatus.Applied)]
    [InlineData(ApplicationStatus.Offer, ApplicationStatus.Interviewing)]
    public void CanTransitionTo_SkippingOrGoingBack_ReturnsFalse(ApplicationStatus current, ApplicationStatus next)
    {
        Assert.False(current.CanTransitionTo(next));
    }

    [Theory]
    [InlineData(ApplicationStatus.Accepted)]
    [InlineData(ApplicationStatus.Rejected)]
    public void CanTransitionTo_FromFinalStatus_ReturnsFalseForEveryStatus(ApplicationStatus finalStatus)
    {
        foreach (var next in Enum.GetValues<ApplicationStatus>())
        {
            Assert.False(finalStatus.CanTransitionTo(next));
        }
    }

    [Theory]
    [InlineData(ApplicationStatus.Applied)]
    [InlineData(ApplicationStatus.Interviewing)]
    [InlineData(ApplicationStatus.Offer)]
    public void CanTransitionTo_SameStatus_ReturnsFalse(ApplicationStatus status)
    {
        Assert.False(status.CanTransitionTo(status));
    }
}
