namespace JobTracker.Domain.JobApplications;

/// <summary>
/// Business rules that govern how an application moves through the hiring process.
/// </summary>
public static class ApplicationStatusExtensions
{
    public static bool CanTransitionTo(this ApplicationStatus current, ApplicationStatus next)
    {
        if (current == ApplicationStatus.Applied)
        {
            return next == ApplicationStatus.Interviewing || next == ApplicationStatus.Rejected;
        }

        if (current == ApplicationStatus.Interviewing)
        {
            return next == ApplicationStatus.Offer || next == ApplicationStatus.Rejected;
        }

        if (current == ApplicationStatus.Offer)
        {
            return next == ApplicationStatus.Accepted || next == ApplicationStatus.Rejected;
        }

        return false;
    }
}
