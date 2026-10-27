using JobApplication.Application.Features.Notifications.Commands;
using JobApplication.Application.Features.Notifications.DTOs;
using JobApplication.Application.Features.Notifications.Queries;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ATS_Checker_Job_Application_Web_Service.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "User,Recruiter,Admin")]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("mine")]
    [ProducesResponseType(typeof(GenericResult<PaginatedResult<NotificationResponseDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? unreadOnly = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMyNotificationsQuery(page, pageSize, unreadOnly), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(GenericResult<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(new GetUnreadNotificationCountQuery(), cancellationToken));

    [HttpPatch("{notificationId:long}/read")]
    public async Task<IActionResult> MarkRead(long notificationId, CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(new MarkNotificationAsReadCommand(notificationId), cancellationToken));

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(new MarkAllNotificationsAsReadCommand(), cancellationToken));

    [HttpDelete("{notificationId:long}")]
    public async Task<IActionResult> Delete(long notificationId, CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(new DeleteNotificationCommand(notificationId), cancellationToken));

    //. Following the DRY principle (hanyaka :) ) 
    private ActionResult ToActionResult<T>(GenericResult<T> result) => result.IsSuccess ? Ok(result) : NotFound(result);
}
