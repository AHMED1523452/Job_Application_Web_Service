using FluentValidation;
using JobApplication.Application.Features.Messages.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Messages.Handlers;

public sealed class MarkMessagesReadHandler : IRequestHandler<MarkMessagesReadCommand, GenericResult<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MarkMessagesReadHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<int>> Handle(MarkMessagesReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<int>("Authentication is required.");

        var participants = _unitOfWork.Repository<ChatParticipant>();
        var participant = await participants.GetByIdAsyncWithTracking(
            row => row.ChatId == request.ChatId && row.UserId == userId.Value,
            cancellationToken);
        if (participant is null)
            return await Result.FailureAsync<int>("Chat not found or access denied.");

        participant.LastReadAtUtc = DateTime.UtcNow;
        var updatedCount = await _unitOfWork.Repository<Message>().ExecuteUpdateAsync(
            message => message.ChatId == request.ChatId
                && message.SenderId != userId.Value
                && !message.IsRead,
            setters => setters.SetProperty(message => message.IsRead, true),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await Result.SuccessAsync(updatedCount, "Messages marked as read.");
    }
}

public class MarkMessagesReadCommandValidator : AbstractValidator<MarkMessagesReadCommand>
{
    public MarkMessagesReadCommandValidator() => RuleFor(command => command.ChatId).NotEmpty();
}
