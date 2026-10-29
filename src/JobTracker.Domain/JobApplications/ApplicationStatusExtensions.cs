namespace JobTracker.Domain.JobApplications;

/// <summary>
/// Business rules that govern how an application moves through the hiring process.
/// </summary>
public static class ApplicationStatusExtensions
{
    /// <summary>
    /// Returns whether an application in <paramref name="current"/> may move to <paramref name="next"/>.
    /// </summary>
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

    /// <summary>
    /// Returns whether the status closes the process (no further transitions allowed).
    /// </summary>
    public static bool IsFinal(this ApplicationStatus status)
    {
        if (status == ApplicationStatus.Accepted || status == ApplicationStatus.Rejected)
        {
            return true;
        }

        return false;
    }
}
