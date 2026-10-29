using JobTracker.Domain.JobApplications;

namespace JobTracker.Api.Contracts;

public sealed record CreateJobApplicationRequest(string Company, string Position, DateOnly AppliedOn);

public sealed record UpdateJobApplicationRequest(string Company, string Position, DateOnly AppliedOn, ApplicationStatus Status);

public sealed record JobApplicationResponse(Guid Id, string Company, string Position, ApplicationStatus Status, DateOnly AppliedOn);
