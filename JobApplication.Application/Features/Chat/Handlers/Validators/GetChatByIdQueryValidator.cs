using FluentValidation;
using JobApplication.Application.Features.Chat.Queries;

namespace JobApplication.Application.Features.Chat.Handlers.Validators;

public sealed class GetChatByIdQueryValidator : AbstractValidator<GetChatByIdQuery>
{
    public GetChatByIdQueryValidator() => RuleFor(query => query.ChatId).NotEmpty();
}
