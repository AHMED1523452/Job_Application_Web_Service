using JobApplication.Application.Features.Candidate.Commands;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Features.Candidate.Queries;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATS_Checker_Job_Application_Web_Service.Controllers;

[ApiController]
[Route("api/candidates")]
public sealed class CandidatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CandidatesController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = "User")]
    [HttpPost]
    [ProducesResponseType(typeof(GenericResult<CandidateResponseDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenericResult<CandidateResponseDTO>>> Create(
        [FromBody] CandidateRequestDTO request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateCandidateCommand(request), cancellationToken);
        if (!result.IsSuccess) return Conflict(result);
        return CreatedAtAction(nameof(GetMine), result);
    }

    [Authorize(Roles = "User")]
    [HttpGet("mine")]
    [ProducesResponseType(typeof(GenericResult<CandidateResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenericResult<CandidateResponseDTO>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyCandidateQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [Authorize(Roles = "User")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GenericResult<CandidateResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenericResult<CandidateResponseDTO>>> Update(
        Guid id,
        [FromBody] CandidateRequestDTO request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateCandidateCommand(id, request), cancellationToken);
        if (result.IsSuccess) return Ok(result);
        if (result.Message == "Candidate profile not found.") return NotFound(result);
        if (result.Message == "You can only update your own candidate profile.") return Forbid();
        return BadRequest(result);
    }

    [Authorize(Roles = "User")]
    [HttpDelete("mine")]
    [ProducesResponseType(typeof(GenericResult<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenericResult<Guid>>> ArchiveMine(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ArchiveCandidateCommand(), cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(GenericResult<PaginatedResult<CandidateResponseDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<GenericResult<PaginatedResult<CandidateResponseDTO>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? orderBy = "CreatedAt",
        [FromQuery] bool orderDesc = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCandidatesQuery(page, pageSize, search, orderBy, orderDesc), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Recruiter,Admin")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GenericResult<CandidateResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenericResult<CandidateResponseDTO>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCandidateByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
}
