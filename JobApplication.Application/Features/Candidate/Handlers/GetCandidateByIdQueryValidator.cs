using FluentValidation;
using JobApplication.Application.Features.Candidate.Queries;

namespace JobApplication.Application.Features.Candidate.Handlers
{
    public class GetCandidateByIdQueryValidator : AbstractValidator<GetCandidateByIdQuery>
    {
        public GetCandidateByIdQueryValidator() => RuleFor(x => x.Id).NotEmpty();
    }
}
