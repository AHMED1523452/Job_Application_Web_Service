using FluentValidation;
using JobApplication.Application.Features.Chat.Commands;
using JobApplication.Application.Features.Chat.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using ChatEntity = JobApplication.Domain.Entities.Chat;

namespace JobApplication.Application.Features.Chat.Handlers;

public sealed class CreateChatHandler : IRequestHandler<CreateChatCommand, GenericResult<CreateChatResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateChatHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<CreateChatResponseDTO>> Handle(CreateChatCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.GetCurrentUserIdOrNull();
        if (!currentUserId.HasValue)
            return await Result.FailureAsync<CreateChatResponseDTO>("Authentication is required.");

        var application = await _unitOfWork.Repository<JobApplications>().GetByIdAsNoTrackingAsync(
            application => application.Id == request.requestDTO.JobApplicationId
                && application.IsActive && !application.IsDeleted && !application.IsArchived,
           selector: application => new ApplicationChatContext
           {
               CandidateProfileId = application.CandidateProfileId,
               CandidateUserId = application.CandidateProfile.UserId,
               HrProfileId = application.Job.HrProfileId,
               RecruiterUserId = application.Job.HrProfile.UserId,
               Status = application.Status
           }, cancellationToken);

        if (application is null)
            return await Result.FailureAsync<CreateChatResponseDTO>("Active job application was not found.");

        if (currentUserId.Value != application.CandidateUserId && currentUserId.Value != application.RecruiterUserId)
            return await Result.FailureAsync<CreateChatResponseDTO>("You are not a participant in this job application.");

        if (application.Status is JobApplicationStatus.Cancelled or JobApplicationStatus.Closed)
            return await Result.FailureAsync<CreateChatResponseDTO>("A chat cannot be opened for a cancelled or closed application.");

        var chatRepository = _unitOfWork.Repository<ChatEntity>();
        var existingChat = await chatRepository.GetByIdAsNoTrackingAsync(
            chat => chat.JobApplicationId == request.requestDTO.JobApplicationId,
            chat => new CreateChatResponseDTO
            {
                Id = chat.Id,
                JobApplicationId = chat.JobApplicationId,
                CandidateProfileId = chat.CandidateProfileId,
                HrProfileId = chat.HrProfileId,
                CandidateUserId = chat.CandidateProfile.UserId,
                RecruiterUserId = chat.HrProfile.UserId,
                CreatedAt = chat.CreatedAt,
                LastMessageAt = chat.LastMessageAt
            }, cancellationToken);

        if (existingChat is not null)
            return await Result.SuccessAsync(existingChat, "Chat already exists.");

        var now = DateTime.UtcNow;
        var newChat = new ChatEntity
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = application.CandidateProfileId,
            HrProfileId = application.HrProfileId,
            JobApplicationId = request.requestDTO.JobApplicationId,
            CreatedAt = now,
            Participants = new List<ChatParticipant>
            {
                new() { UserId = application.CandidateUserId, JoinedAtUtc = now },
                new() { UserId = application.RecruiterUserId, JoinedAtUtc = now }
            }
        };

        await chatRepository.AddAsync(newChat, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await Result.SuccessAsync(new CreateChatResponseDTO
        {
            Id = newChat.Id,
            JobApplicationId = newChat.JobApplicationId,
            CandidateProfileId = newChat.CandidateProfileId,
            HrProfileId = newChat.HrProfileId,
            CandidateUserId = application.CandidateUserId,
            RecruiterUserId = application.RecruiterUserId,
            CreatedAt = newChat.CreatedAt
        }, "Chat created.");
    }
}

public class CreateChatCommandValidator : AbstractValidator<CreateChatCommand>
{
    public CreateChatCommandValidator() => RuleFor(request => request.requestDTO.JobApplicationId).NotEmpty();
}
