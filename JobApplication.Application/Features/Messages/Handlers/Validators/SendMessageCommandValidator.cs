using FluentValidation;
using JobApplication.Application.Features.Messages.Commands;

namespace JobApplication.Application.Features.Messages.Handlers.Validators;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(command => command.ChatId).NotEmpty();
        RuleFor(command => command.Request)
            .NotNull()
            .DependentRules(() =>
            {
                RuleFor(command => command.Request.Content).MaximumLength(5000);
                RuleFor(command => command.Request)
                    .Must(request => !string.IsNullOrWhiteSpace(request.Content) || request.MediaFileId.HasValue)
                    .WithMessage("Provide message content, an attachment, or both.");
                RuleFor(command => command.Request.MediaFileId)
                    .Must(id => !id.HasValue || id.Value != Guid.Empty)
                    .WithMessage("MediaFileId must be a valid identifier.");
            });
    }
}
