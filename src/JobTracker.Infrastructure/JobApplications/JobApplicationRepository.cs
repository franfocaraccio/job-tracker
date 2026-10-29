using JobTracker.Application.JobApplications;
using JobTracker.Domain.JobApplications;
using Npgsql;

namespace JobTracker.Infrastructure.JobApplications;

/// <summary>
/// Stores job applications in PostgreSQL using the Npgsql ADO.NET provider.
/// </summary>
public sealed class JobApplicationRepository : IJobApplicationRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public JobApplicationRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IReadOnlyList<JobApplication>> GetByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        await using var command = _dataSource.CreateCommand("SELECT id, user_id, company, position, status, applied_on FROM job_applications " + "WHERE user_id = @userId ORDER BY applied_on DESC");
        command.Parameters.AddWithValue("userId", userId);

        var applications = new List<JobApplication>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            applications.Add(ReadApplication(reader));
        }

        return applications;
    }

    public async Task<JobApplication?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await using var command = _dataSource.CreateCommand("SELECT id, user_id, company, position, status, applied_on FROM job_applications " + "WHERE id = @id AND user_id = @userId");
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("userId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadApplication(reader);
    }

    public async Task AddAsync(JobApplication application, CancellationToken cancellationToken)
    {
        await using var command = _dataSource.CreateCommand("INSERT INTO job_applications (id, user_id, company, position, status, applied_on) " + "VALUES (@id, @userId, @company, @position, @status, @appliedOn)");
        command.Parameters.AddWithValue("id", application.Id);
        command.Parameters.AddWithValue("userId", application.UserId);
        command.Parameters.AddWithValue("company", application.Company);
        command.Parameters.AddWithValue("position", application.Position);
        command.Parameters.AddWithValue("status", application.Status.ToString());
        command.Parameters.AddWithValue("appliedOn", application.AppliedOn);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(JobApplication application, CancellationToken cancellationToken)
    {
        await using var command = _dataSource.CreateCommand("UPDATE job_applications " + "SET company = @company, position = @position, status = @status, applied_on = @appliedOn " + "WHERE id = @id");
        command.Parameters.AddWithValue("id", application.Id);
        command.Parameters.AddWithValue("company", application.Company);
        command.Parameters.AddWithValue("position", application.Position);
        command.Parameters.AddWithValue("status", application.Status.ToString());
        command.Parameters.AddWithValue("appliedOn", application.AppliedOn);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var command = _dataSource.CreateCommand("DELETE FROM job_applications WHERE id = @id");
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static JobApplication ReadApplication(NpgsqlDataReader reader)
    {
        return JobApplication.FromDatabase(
            reader.GetGuid(0),
            reader.GetGuid(1),
            reader.GetString(2),
            reader.GetString(3),
            Enum.Parse<ApplicationStatus>(reader.GetString(4)),
            reader.GetFieldValue<DateOnly>(5));
    }
}
