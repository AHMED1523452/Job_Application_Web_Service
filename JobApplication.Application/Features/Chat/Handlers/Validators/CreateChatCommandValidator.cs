using FluentValidation;
using JobApplication.Application.Features.Chat.Commands;

namespace JobApplication.Application.Features.Chat.Handlers.Validators;

public sealed class CreateChatCommandValidator : AbstractValidator<CreateChatCommand>
{
    public CreateChatCommandValidator() => RuleFor(request => request.Request.JobApplicationId).NotEmpty();
}
