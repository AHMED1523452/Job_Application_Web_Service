using JobApplication.Application.Features.JobApplication.Commands;
using JobApplication.Application.Features.JobApplication.DTOs;
using JobApplication.Application.Features.JobApplication.Queries;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATS_Checker_Job_Application_Web_Service.Controllers;

[ApiController]
[Route("api/job-applications")]
[Authorize]
public sealed class JobApplicationController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobApplicationController(IMediator mediator) => _mediator = mediator;

    [HttpGet("jobs/{jobId:guid}")]
    [Authorize(Roles = "Recruiter")]
    [ProducesResponseType(typeof(GenericResult<PaginatedResult<JobApplicationResponseDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<GenericResult<PaginatedResult<JobApplicationResponseDTO>>>> GetJobApplications(
        Guid jobId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool orderDesc = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetJobApplicationsQuery(jobId, page, pageSize, search, orderBy, orderDesc),
            cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("candidates/{candidateProfileId:guid}/{applicationId:guid}")]
    [ProducesResponseType(typeof(GenericResult<JobApplicationResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<GenericResult<JobApplicationResponseDTO>>> GetCandidateApplication(
        Guid candidateProfileId,
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetCandidateJobApplicationByIdQuery(candidateProfileId, applicationId), cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpPost("candidates/{candidateProfileId:guid}")]
    [ProducesResponseType(typeof(GenericResult<CreateJobApplicationResponseDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenericResult<CreateJobApplicationResponseDTO>>> Create(
        Guid candidateProfileId,
        [FromBody] JobApplicationRequestDTO request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateJobApplicationCommand(candidateProfileId, request), cancellationToken);
        return result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result) : BadRequest(result);
    }

    [HttpGet("candidates/{candidateProfileId:guid}")]
    [ProducesResponseType(typeof(GenericResult<PaginatedResult<JobApplicationResponseDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<GenericResult<PaginatedResult<JobApplicationResponseDTO>>>> GetCandidateApplications(
        Guid candidateProfileId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool orderDesc = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetUserJobApplicationsQuery(candidateProfileId, page, pageSize, search, orderBy, orderDesc),
            cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("candidates/{candidateProfileId:guid}/{applicationId:guid}")]
    [ProducesResponseType(typeof(GenericResult<UpdateApplicationResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<GenericResult<UpdateApplicationResponseDTO>>> Update(
        Guid candidateProfileId,
        Guid applicationId,
        [FromBody] UpdateJobApplicationRequestDTO request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateJobApplicationCommand(candidateProfileId, applicationId, request), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("candidates/{candidateProfileId:guid}/{applicationId:guid}/cancel")]
    [ProducesResponseType(typeof(GenericResult<CancelJobApplicationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<GenericResult<CancelJobApplicationResponse>>> Cancel(
        Guid candidateProfileId,
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CancelJobApplicationCommand(candidateProfileId, applicationId), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{applicationId:guid}/status")]
    [Authorize(Roles = "Recruiter")]
    [ProducesResponseType(typeof(GenericResult<CloseJobApplicationResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<GenericResult<CloseJobApplicationResponseDTO>>> ChangeStatus(
        Guid applicationId,
        [FromBody] ChangeJobApplicationStatusRequestDTO request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ChangeJobApplicationStatusCommand(applicationId, request), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{applicationId:guid}/close")]
    [Authorize(Roles = "Recruiter")]
    [ProducesResponseType(typeof(GenericResult<CloseJobApplicationResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<GenericResult<CloseJobApplicationResponseDTO>>> Close(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CloseJobApplicationCommand(applicationId), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
