using JobApplication.Application.Features.Messages.Commands;
using JobApplication.Application.Features.Messages.DTOs;
using JobApplication.Application.Features.Messages.Queries;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATS_Checker_Job_Application_Web_Service.Controllers;

[ApiController]
[Route("api/chats/{chatId:guid}/messages")]
[Authorize(Roles = "User,Recruiter")]
public sealed class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;
    public MessagesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(GenericResult<PaginatedResult<MessageResponseDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChatMessages(
        Guid chatId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _mediator.Send(new GetChatMessagesQuery(chatId, page, pageSize), cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(GenericResult<MessageResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMessage(
        Guid chatId,
        [FromBody] SendMessageRequestDTO request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(new SendMessageCommand(chatId, request), cancellationToken));

    [HttpPatch("read")]
    [ProducesResponseType(typeof(GenericResult<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkMessagesRead(Guid chatId, CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(new MarkMessagesReadCommand(chatId), cancellationToken));

    private ActionResult ToActionResult<T>(GenericResult<T> result) =>
        result.IsSuccess ? Ok(result) : BadRequest(result);
}
