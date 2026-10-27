using JobApplication.Application.Features.Skills.Commands;
using JobApplication.Application.Features.Skills.DTOs;
using JobApplication.Application.Features.Skills.Queries;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ATS_Checker_Job_Application_Web_Service.Controllers
{
    [ApiController]
    [Route("api/skills")]
    public class SkillsController : ControllerBase
    {
        private readonly IMediator mediator;

        public SkillsController(IMediator mediator )
        {
            this.mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(GenericResult<CreateSkillsResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Recruiter")]
        public async Task<ActionResult<GenericResult<CreateSkillsResponseDTO>>> 
                            CreateAsync(CreateSkillsRequestDTO requestDTO, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CreateSkillsCommand(requestDTO), cancellationToken);
            if (!result.IsSuccess) return Conflict(result);

            return Created(); //. 201 response 
        }

        [HttpPut("{Id}")]
        [ProducesResponseType(typeof(GenericResult<CreateSkillsResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Recruiter")]
        public async Task<ActionResult<GenericResult<CreateSkillsResponseDTO>>> 
                            UpdateAsync(Guid Id, CreateSkillsRequestDTO requestDTO, CancellationToken cancellationToken)
        {   
            var result = await mediator.Send(new UpdateSkillsCommand(Id,requestDTO), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result); 
        }
        [HttpPatch("{Id:guid}/remove")]
        [ProducesResponseType(typeof(GenericResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Recruiter")]
        public async Task<ActionResult<GenericResult<CreateSkillsResponseDTO>>> 
                            RemoveAsync(Guid Id,CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteSkillCommand(Id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result); 
        }

        [HttpGet]
        [ProducesResponseType(typeof(GenericResult<PaginatedResult<SkillsDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<ActionResult<GenericResult<PaginatedResult<SkillsDTO>>>> 
                            GetSkills([FromQuery] int page,
                                      [FromQuery] int pageSize,
                                      [FromQuery] string? search,
                                      CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetSkillsQuery(page, pageSize, search), cancellationToken);
            if (!result.IsSuccess) return NotFound(result);

            return Ok(result); 
        }

        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(GenericResult<SkillsDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<ActionResult<GenericResult<CreateSkillsResponseDTO>>> 
                            GetByIdAsync(Guid Id,CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetSkillByIdQuery(Id), cancellationToken);
            if (!result.IsSuccess) return NotFound(result);

            return Ok(result); 
        }

    }
}
