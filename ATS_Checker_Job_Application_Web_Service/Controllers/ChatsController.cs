using JobApplication.Application.Features.Chat.Commands;
using JobApplication.Application.Features.Chat.DTOs;
using JobApplication.Application.Features.Chat.Queries;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATS_Checker_Job_Application_Web_Service.Controllers;

[ApiController]
[Route("api/chats")]
[Authorize(Roles = "User,Recruiter")]
public sealed class ChatsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ChatsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(typeof(GenericResult<CreateChatResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateChatRequestDTO request, CancellationToken ct) =>
        ToActionResult(await _mediator.Send(new CreateChatCommand(request), ct));

    [HttpGet("mine")]
    [ProducesResponseType(typeof(GenericResult<PaginatedResult<ChatResponseDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        ToActionResult(await _mediator.Send(new GetMyChatsQuery(page, pageSize), ct));

    [HttpGet("{chatId:guid}")]
    [ProducesResponseType(typeof(GenericResult<ChatResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid chatId, CancellationToken ct) =>
        ToActionResult(await _mediator.Send(new GetChatByIdQuery(chatId), ct));

    private ActionResult ToActionResult<T>(GenericResult<T> result) =>
        result.IsSuccess ? Ok(result) : BadRequest(result);
}
