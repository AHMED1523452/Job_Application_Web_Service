using JobApplication.Application.Features.Messages.Commands;
using JobApplication.Application.Features.Messages.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;
using ChatEntity = JobApplication.Domain.Entities.Chat;

namespace JobApplication.Application.Features.Messages.Handlers;

public sealed class SendMessageHandler : IRequestHandler<SendMessageCommand, GenericResult<MessageResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SendMessageHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<MessageResponseDTO>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<MessageResponseDTO>("Authentication is required.");

        if (!await _unitOfWork.Repository<ChatParticipant>().AnyAsync(
                participant => participant.ChatId == request.ChatId && participant.UserId == userId.Value,
                cancellationToken))
            return await Result.FailureAsync<MessageResponseDTO>("Chat not found or access denied.");

        // The upload flow must save MediaFile first; this verifies its returned ID is valid and usable.
        if (request.Request.MediaFileId.HasValue && !await _unitOfWork.Repository<MediaFile>().AnyAsync(
                file => file.MediaFileID == request.Request.MediaFileId.Value && !file.IsDeleted && !file.IsArchived,
                cancellationToken))
            return await Result.FailureAsync<MessageResponseDTO>("Attachment was not found or is unavailable.");

        var chat = await _unitOfWork.Repository<ChatEntity>()
            .GetByIdAsyncWithTracking(entity => entity.Id == request.ChatId, cancellationToken);
        if (chat is null)
            return await Result.FailureAsync<MessageResponseDTO>("Chat not found.");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = request.ChatId,
            SenderId = userId.Value,
            Content = request.Request.Content?.Trim() ?? string.Empty,
            MediaFileId = request.Request.MediaFileId,
            IsRead = false,
            SentAt = DateTime.UtcNow
        };

        chat.LastMessageAt = message.SentAt;
        await _unitOfWork.Repository<Message>().AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await Result.SuccessAsync(new MessageResponseDTO
        {
            Id = message.Id,
            ChatId = message.ChatId,
            SenderId = message.SenderId,
            Content = message.Content,
            MediaFileId = message.MediaFileId,
            IsRead = message.IsRead,
            SentAt = message.SentAt
        }, "Message sent.");
    }
}
