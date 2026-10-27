using JobApplication.Application.Features.HrProfile.Commands;
using JobApplication.Application.Features.HrProfile.DTOs;
using JobApplication.Application.Features.HrProfile.Queries;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ATS_Checker_Job_Application_Web_Service.Controllers;

[ApiController]
[Route("api/hrprofile")]
[Authorize(Roles = "Recruiter")]
public sealed class HrProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public HrProfileController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get the current recruiter's HR profile with company details.
    /// Uses Select projection (no eager loading) - reads only required fields.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GenericResult<HrProfileResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EnableRateLimiting("Users")]
    public async Task<ActionResult<GenericResult<HrProfileResponseDTO>>> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetHrProfileQuery(), cancellationToken);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Update the current recruiter's HR profile and company details.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(GenericResult<HrProfileResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting("Users")]
    public async Task<ActionResult<GenericResult<HrProfileResponseDTO>>> UpdateProfile(
        [FromBody] UpdateHrProfileRequestDTO request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateHrProfileCommand(request), cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Upload company logo for the current recruiter's HR profile.
    /// If a logo already exists, returns the existing logo path with a message (does not overwrite).
    /// Rate limited to 5 requests per minute (Upload policy).
    /// </summary>
    [HttpPost("logo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(GenericResult<UploadCompanyLogoResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting("Upload")]
    public async Task<ActionResult<GenericResult<UploadCompanyLogoResponseDTO>>> UploadCompanyLogo(
        [FromForm] UploadCompanyLogoCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}