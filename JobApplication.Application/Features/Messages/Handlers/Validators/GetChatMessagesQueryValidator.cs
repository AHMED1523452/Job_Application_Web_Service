using FluentValidation;
using JobApplication.Application.Features.Messages.Queries;

namespace JobApplication.Application.Features.Messages.Handlers.Validators;

public sealed class GetChatMessagesQueryValidator : AbstractValidator<GetChatMessagesQuery>
{
    public GetChatMessagesQueryValidator()
    {
        RuleFor(query => query.ChatId).NotEmpty();
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
