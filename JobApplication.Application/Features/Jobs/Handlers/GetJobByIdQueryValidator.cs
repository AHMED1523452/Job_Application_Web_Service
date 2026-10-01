using FluentValidation;
using JobApplication.Application.Features.Jobs.Queries;

namespace JobApplication.Application.Features.Jobs.Handlers
{
    public sealed class GetJobByIdQueryValidator : AbstractValidator<GetJobByIdQuery>
    {
        public GetJobByIdQueryValidator() => RuleFor(x => x.Id).NotEmpty();
    }
}
