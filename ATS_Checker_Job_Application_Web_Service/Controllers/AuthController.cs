using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Features.Auth.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ATS_Checker_Job_Application_Web_Service.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("Auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        [HttpPost("register/candidate")]
        [ProducesResponseType(typeof(GenericResult<RegisterResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<RegisterResponseDTO>>> Register([FromBody] RegisterRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RegisterCommand(request), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("register/recruiter")]
        [ProducesResponseType(typeof(GenericResult<RegisterResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<RegisterResponseDTO>>> RegisterRecruiter(
            [FromBody] RecruiterRegisterRequestDTO request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RegisterRecruiterCommand(request), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        //. Admin must registered for another admin 
        [Authorize(Roles = "Admin")]
        [HttpPost("register/admin")]
        [ProducesResponseType(typeof(GenericResult<RegisterResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GenericResult<RegisterResponseDTO>>> RegisterAdmin(
            [FromBody] RegisterRequestDTO request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RegisterAdminCommand(request), cancellationToken);
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

        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(GenericResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<string>>> VerifyEmail([FromBody] VerifyEmailRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new VerifyEmailCommand(request), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(GenericResult<LoginResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<LoginResponseDTO>>> Refresh([FromBody] RefreshTokenRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(GenericResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<string>>> ForgotPassword([FromBody] ForgotPasswordRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ForgotPasswordCommand(request), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(GenericResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<string>>> ResetPassword([FromBody] ResetPasswordRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ResetPasswordCommand(request), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(GenericResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GenericResult<string>>> ChangePassword(
            [FromBody] ChangePasswordRequestDTO request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ChangePasswordCommand(request), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("logout")]
        [ProducesResponseType(typeof(GenericResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GenericResult<string>>> Logout([FromBody] LogoutRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new LogoutCommand(request), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("revoke-user")]
        [ProducesResponseType(typeof(GenericResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GenericResult<string>>> RevokeUser([FromBody] RevokeUserRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RevokeUserCommand(request), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("unrevoke-user")]
        [ProducesResponseType(typeof(GenericResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GenericResult<string>>> UnRevokeUser([FromBody] UnRevokeUserRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new UnRevokeUserCommand(request), cancellationToken);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}
