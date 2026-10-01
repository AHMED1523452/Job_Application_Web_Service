using FluentValidation;
using JobApplication.Application.Features.JobApplication.Commands;

namespace JobApplication.Application.Features.JobApplication.Handlers;

public sealed class CancelJobApplicationValidator : AbstractValidator<CancelJobApplicationCommand>
{
    public CancelJobApplicationValidator()
    {
        RuleFor(command => command.candidateProfileId).NotEmpty();
        RuleFor(command => command.appId).NotEmpty();
    }
}
