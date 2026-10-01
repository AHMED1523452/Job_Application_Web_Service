using FluentValidation;
using JobApplication.Application.Features.JobApplication.Queries;

namespace JobApplication.Application.Features.JobApplication.Handlers;

public sealed class GetJobApplicationsQueryValidator : AbstractValidator<GetJobApplicationsQuery>
{
    public GetJobApplicationsQueryValidator()
    {
        RuleFor(query => query.JobId).NotEmpty();
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Search).MaximumLength(100);
        RuleFor(query => query.OrderBy)
            .Must(orderBy => string.IsNullOrWhiteSpace(orderBy)
                || new[] { "AppliedAtUtc", "Id", "Status" }.Contains(orderBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage("OrderBy must be AppliedAtUtc, Id, or Status.");
    }
}
