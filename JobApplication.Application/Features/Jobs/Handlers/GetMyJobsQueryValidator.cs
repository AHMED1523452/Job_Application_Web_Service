using FluentValidation;
using JobApplication.Application.Features.Jobs.Queries;

namespace JobApplication.Application.Features.Jobs.Handlers
{
    public sealed class GetMyJobsQueryValidator : AbstractValidator<GetMyJobsQuery>
    {
        public GetMyJobsQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
