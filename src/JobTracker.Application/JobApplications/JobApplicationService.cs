using JobTracker.Domain.JobApplications;

namespace JobTracker.Application.JobApplications;

/// <summary>
/// Use cases for managing the job applications of the user.
/// </summary>
public sealed class JobApplicationService
{
    private readonly IJobApplicationRepository _repository;

    public JobApplicationService(IJobApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<JobApplication>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _repository.GetByUserAsync(userId, cancellationToken);
    }

    public async Task<JobApplication?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(id, userId, cancellationToken);
    }

    public async Task<JobApplication> CreateAsync(
        Guid userId,
        string company,
        string position,
        DateOnly appliedOn,
        CancellationToken cancellationToken)
    {
        var application = JobApplication.Create(userId, company, position, appliedOn);

        await _repository.AddAsync(application, cancellationToken);

        return application;
    }

    public async Task<JobApplication?> UpdateAsync(
        Guid id,
        Guid userId,
        string company,
        string position,
        DateOnly appliedOn,
        ApplicationStatus status,
        CancellationToken cancellationToken)
    {
        var application = await _repository.GetByIdAsync(id, userId, cancellationToken);
        if (application is null)
        {
            return null;
        }

        application.UpdateDetails(company, position, appliedOn);

        if (application.Status != status)
        {
            application.ChangeStatus(status);
        }

        await _repository.UpdateAsync(application, cancellationToken);

        return application;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var application = await _repository.GetByIdAsync(id, userId, cancellationToken);
        if (application is null)
        {
            return false;
        }

        await _repository.DeleteAsync(application.Id, cancellationToken);

        return true;
    }
}
