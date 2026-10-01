using FluentValidation;
using JobApplication.Application.Features.Messages.Commands;

namespace JobApplication.Application.Features.Messages.Handlers.Validators;

public sealed class MarkMessagesReadCommandValidator : AbstractValidator<MarkMessagesReadCommand>
{
    public MarkMessagesReadCommandValidator() => RuleFor(command => command.ChatId).NotEmpty();
}
