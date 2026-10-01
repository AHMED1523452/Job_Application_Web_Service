using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Features.Jobs.Queries;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATS_Checker_Job_Application_Web_Service.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public sealed class JobsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobsController(IMediator mediator) => _mediator = mediator;

        // Public job discovery, with filters, pagination, and optional geographic search.
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<JobListItemsResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<JobListItemsResponseDTO>>> GetAll(
            [FromQuery] JobSearchRequestDTO request,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetJobsQuery(request, page, pageSize), cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(GenericResult<JobDetailsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenericResult<JobDetailsResponseDTO>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetJobByIdQuery(id), cancellationToken);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }

        [Authorize(Roles = "Recruiter")]
        [HttpPost]
        [ProducesResponseType(typeof(GenericResult<JobDetailsResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<JobDetailsResponseDTO>>> Create(
            [FromBody] JobRequestDTO request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CreateJobCommand(request), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        // POST supports clients that prefer a structured search body, especially for SkillIds.
        [AllowAnonymous]
        [HttpPost("search")]
        [ProducesResponseType(typeof(PaginatedResult<JobListItemsResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<JobListItemsResponseDTO>>> Search(
            [FromBody] JobSearchRequestDTO request,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetJobsQuery(request, page, pageSize), cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("{id:guid}/views")]
        [ProducesResponseType(typeof(GenericResult<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<GenericResult<int>>> RecordView(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RecordJobViewCommand(id), cancellationToken);
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }

        [Authorize(Roles = "Recruiter")]
        [HttpGet("mine")]
        [ProducesResponseType(typeof(PaginatedResult<JobListItemsResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<JobListItemsResponseDTO>>> GetMine(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetMyJobsQuery(page, pageSize), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Recruiter")]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(GenericResult<JobDetailsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<JobDetailsResponseDTO>>> Update(
            Guid id,
            [FromBody] JobRequestDTO request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new UpdateJobCommand(id, request), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Recruiter")]
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(GenericResult<JobDetailsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<JobDetailsResponseDTO>>> ChangeStatus(
            Guid id,
            [FromBody] ChangeJobStatusRequestDTO request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ChangeJobStatusCommand(id, request), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Recruiter")]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(GenericResult<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<Guid>>> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteJobCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("saved")]
        [ProducesResponseType(typeof(PaginatedResult<JobListItemsResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<JobListItemsResponseDTO>>> GetSaved(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetSavedJobsQuery(page, pageSize), cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("{id:guid}/save")]
        [ProducesResponseType(typeof(GenericResult<JobListItemsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<JobListItemsResponseDTO>>> Save(
            Guid id,
            [FromBody] SavedJobRequestDTO request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new SaveJobCommand(id, request), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id:guid}/un-save")]
        [ProducesResponseType(typeof(GenericResult<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<Guid>>> Unsave(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RemoveSavedJobCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}
