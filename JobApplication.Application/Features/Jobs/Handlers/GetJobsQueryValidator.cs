using FluentValidation;
using JobApplication.Application.Features.Jobs.Queries;

namespace JobApplication.Application.Features.Jobs.Handlers
{

    public class GetJobsQueryValidator : AbstractValidator<GetJobsQuery>
    {
        public GetJobsQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
