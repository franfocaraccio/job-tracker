using System.ComponentModel.DataAnnotations;

namespace JobTracker.Domain.JobApplications;

/// <summary>
/// A position a user applied to, tracked through the hiring process.
/// </summary>
public sealed class JobApplication
{
    public const int MaxTextLength = 100;

    private JobApplication(
        Guid id,
        Guid userId,
        string company,
        string position,
        ApplicationStatus status,
        DateOnly appliedOn)
    {
        Id = id;
        UserId = userId;
        Company = company;
        Position = position;
        Status = status;
        AppliedOn = appliedOn;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public string Company { get; private set; }

    public string Position { get; private set; }

    public ApplicationStatus Status { get; private set; }

    public DateOnly AppliedOn { get; private set; }

    public static JobApplication Create(Guid userId, string company, string position, DateOnly appliedOn)
    {
        if (userId == Guid.Empty)
        {
            throw new ValidationException("An application must belong to a user.");
        }

        ValidateDetails(company, position, appliedOn);

        return new JobApplication(
            Guid.NewGuid(),
            userId,
            company.Trim(),
            position.Trim(),
            ApplicationStatus.Applied,
            appliedOn);
    }

    public static JobApplication FromDatabase(
        Guid id,
        Guid userId,
        string company,
        string position,
        ApplicationStatus status,
        DateOnly appliedOn)
    {
        return new JobApplication(id, userId, company, position, status, appliedOn);
    }

    public void UpdateDetails(string company, string position, DateOnly appliedOn)
    {
        ValidateDetails(company, position, appliedOn);

        Company = company.Trim();
        Position = position.Trim();
        AppliedOn = appliedOn;
    }

    public void ChangeStatus(ApplicationStatus newStatus)
    {
        if (!Status.CanTransitionTo(newStatus))
        {
            throw new ValidationException($"An application cannot move from '{Status}' to '{newStatus}'.");
        }

        Status = newStatus;
    }

    private static void ValidateDetails(string company, string position, DateOnly appliedOn)
    {
        ValidateText(company, "Company");
        ValidateText(position, "Position");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (appliedOn > today)
        {
            throw new ValidationException("The application date cannot be in the future.");
        }
    }

    private static void ValidateText(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{fieldName} is required.");
        }

        if (value.Trim().Length > MaxTextLength)
        {
            throw new ValidationException($"{fieldName} must be {MaxTextLength} characters or fewer.");
        }
    }
}
