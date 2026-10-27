using JobApplication.Application.Features.Users.Commands;
using JobApplication.Application.Features.Users.DTOs;
using JobApplication.Application.Features.Users.Queries;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ATS_Checker_Job_Application_Web_Service.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    [EnableRateLimiting("Users")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(GenericResult<List<UserResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GenericResult<List<UserResponseDTO>>>> GetUsers(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetUsersQuery(), cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{userId:guid}")]
        [ProducesResponseType(typeof(GenericResult<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenericResult<UserResponseDTO>>> GetUserById(
            Guid userId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetUserByIdQuery(userId), cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("{userId:guid}")]
        [ProducesResponseType(typeof(GenericResult<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenericResult<UserResponseDTO>>> UpdateUser(
            Guid userId,
            [FromBody] UpdateUserRequestDTO request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new UpdateUserCommand(userId, request), cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{userId:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(GenericResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenericResult<bool>>> DeleteUser(
            Guid userId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteUserCommand(userId), cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
    }
}
