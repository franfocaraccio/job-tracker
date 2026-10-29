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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns whether the status closes the process (no further transitions allowed).
    /// </summary>
    public static bool IsFinal(this ApplicationStatus status)
    {
        throw new NotImplementedException();
    }
}
