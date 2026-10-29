namespace JobTracker.Domain.JobApplications;

/// <summary>
/// Stage of a job application within the hiring process.
/// </summary>
public enum ApplicationStatus
{
    Applied,
    Interviewing,
    Offer,
    Accepted,
    Rejected,
}
