using JobApplication.Application.Auth.Commands;
using JobApplication.Application.Auth.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATS_Checker_Job_Application_Web_Service.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(GenericResult<RegisterResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<RegisterResponseDTO>>> Register([FromBody] RegisterRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RegisterCommand(request), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(GenericResult<LoginResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<LoginResponseDTO>>> Login([FromBody] LoginRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new LoginCommand(request), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
