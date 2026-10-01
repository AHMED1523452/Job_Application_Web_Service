using FluentValidation;
using JobApplication.Application.Features.Chat.Queries;

namespace JobApplication.Application.Features.Chat.Handlers.Validators;

public sealed class GetMyChatsQueryValidator : AbstractValidator<GetMyChatsQuery>
{
    public GetMyChatsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
