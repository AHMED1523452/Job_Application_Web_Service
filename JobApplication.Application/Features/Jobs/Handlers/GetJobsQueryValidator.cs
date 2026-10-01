using FluentValidation;
using JobApplication.Application.Features.Jobs.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Features.Jobs.DTOs;

namespace JobApplication.Application.Features.Jobs.Handlers
{
    public sealed class GetJobsQueryValidator : AbstractValidator<GetJobsQuery>
    {
        public GetJobsQueryValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Request).NotNull().SetValidator(new JobSearchRequestDTOValidator(unitOfWork));
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
