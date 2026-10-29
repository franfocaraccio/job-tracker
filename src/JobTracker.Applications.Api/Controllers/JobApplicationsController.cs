using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using JobTracker.Application.JobApplications;
using JobTracker.Applications.Api.Contracts;
using JobTracker.Domain.JobApplications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace JobTracker.Applications.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/applications")]
public sealed class JobApplicationsController : ControllerBase
{
    private readonly JobApplicationService _service;

    public JobApplicationsController(JobApplicationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var applications = await _service.GetAllAsync(GetUserId(), cancellationToken);

        return Ok(applications.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var application = await _service.GetByIdAsync(id, GetUserId(), cancellationToken);
        if (application is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(application));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateJobApplicationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var application = await _service.CreateAsync(
                GetUserId(), request.Company, request.Position, request.AppliedOn, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = application.Id }, ToResponse(application));
        }
        catch (ValidationException exception)
        {
            return Problem(detail: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateJobApplicationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var application = await _service.UpdateAsync(
                id, GetUserId(), request.Company, request.Position, request.AppliedOn, request.Status, cancellationToken);
            if (application is null)
            {
                return NotFound();
            }

            return Ok(ToResponse(application));
        }
        catch (ValidationException exception)
        {
            return Problem(detail: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, GetUserId(), cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    }

    private static JobApplicationResponse ToResponse(JobApplication application)
    {
        return new JobApplicationResponse(
            application.Id,
            application.Company,
            application.Position,
            application.Status,
            application.AppliedOn);
    }
}
