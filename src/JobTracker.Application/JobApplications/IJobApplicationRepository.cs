using JobTracker.Domain.JobApplications;

namespace JobTracker.Application.JobApplications;

public interface IJobApplicationRepository
{
    Task<IReadOnlyList<JobApplication>> GetByUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<JobApplication?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    Task AddAsync(JobApplication application, CancellationToken cancellationToken);

    Task UpdateAsync(JobApplication application, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
