using FluentValidation;
using JobApplication.Application.Features.Candidate.Queries;

namespace JobApplication.Application.Features.Candidate.Handlers;

public sealed class GetCandidatesQueryValidator : AbstractValidator<GetCandidatesQuery>
{
    private static readonly string[] AllowedOrderBy = ["CreatedAt", "Title", "YearsOfExperience", "Location"];

    public GetCandidatesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(150);
        RuleFor(x => x.OrderBy).Must(value => string.IsNullOrWhiteSpace(value) || AllowedOrderBy.Contains(value))
            .WithMessage($"OrderBy must be one of: {string.Join(", ", AllowedOrderBy)}.");
    }
}
