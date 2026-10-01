using FluentValidation;
using JobApplication.Application.Features.JobApplication.Queries;

namespace JobApplication.Application.Features.JobApplication.Handlers;

public sealed class GetCandidateJobApplicationByIdValidator : AbstractValidator<GetCandidateJobApplicationByIdQuery>
{
    public GetCandidateJobApplicationByIdValidator()
    {
        RuleFor(query => query.CandidateProfileId).NotEmpty();
        RuleFor(query => query.ApplicationId).NotEmpty();
    }
}
