using JobApplication.Application.JobApplication.Commands;
using JobApplication.Application.JobApplication.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ATS_Checker_Job_Application_Web_Service.Controllers
{
    [ApiController]
    [Route("api/job-applications")]
    public class JobApplicationController : ControllerBase
    {
        private readonly IMediator mediator;

        public JobApplicationController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(GenericResult<JobApplicationRequestDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<ActionResult<GenericResult<CreateJobApplicationResponseDTO>>> CreateJobApplication([FromBody] JobApplicationRequestDTO request,
                                                                                                             CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CreateJobApplicationCommand(request), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(GenericResult<UpdateApplicationResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<ActionResult<GenericResult<UpdateApplicationResponseDTO>>> UpdateAsync(Guid id,
                                                                                                [FromBody] UpdateJobApplicationRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new UpdateJobApplicationCommand(id, request), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);

        }

        [HttpPatch("{Id}/cancel")]
        [ProducesResponseType(typeof(GenericResult<CancelJobApplicationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<ActionResult<GenericResult<CancelJobApplicationResponse>>> CancelApplication(Guid Id, 
                                                                                                       CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CancelJobApplicationCommand(Id), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }
        
        [HttpPatch("{Id}/close-application")]
        [ProducesResponseType(typeof(GenericResult<CloseJobApplicationResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<ActionResult<GenericResult<CloseJobApplicationResponseDTO>>> CloseJobApplicationAsync(Guid Id, 
                                                                                                       CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CloseJobApplicationCommand(Id), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
