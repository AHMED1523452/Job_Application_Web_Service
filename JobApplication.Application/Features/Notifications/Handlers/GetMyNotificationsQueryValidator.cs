using FluentValidation;
using JobApplication.Application.Features.Notifications.Queries;

namespace JobApplication.Application.Features.Notifications.Handlers;

public sealed class GetMyNotificationsQueryValidator : AbstractValidator<GetMyNotificationsQuery>
{
    public GetMyNotificationsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
