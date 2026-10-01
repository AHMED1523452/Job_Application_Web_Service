using FluentValidation;
using JobApplication.Application.Features.JobApplication.Commands;

namespace JobApplication.Application.Features.JobApplication.Handlers;

public sealed class CloseJobApplicationValidator : AbstractValidator<CloseJobApplicationCommand>
{
    public CloseJobApplicationValidator() => RuleFor(command => command.appId).NotEmpty();
}
