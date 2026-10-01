using FluentValidation;
using JobApplication.Application.Features.JobApplication.Commands;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.JobApplication.Handlers;

public sealed class ChangeJobApplicationStatusValidator : AbstractValidator<ChangeJobApplicationStatusCommand>
{
    private static readonly JobApplicationStatus[] RecruiterStatuses =
    [
        JobApplicationStatus.UnderReview,
        JobApplicationStatus.InterviewScheduled,
        JobApplicationStatus.Accepted,
        JobApplicationStatus.Rejected
    ];

    public ChangeJobApplicationStatusValidator()
    {
        RuleFor(command => command.ApplicationId).NotEmpty();
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Status)
            .Must(status => RecruiterStatuses.Contains(status))
            .WithMessage("Recruiters can set status to UnderReview, InterviewScheduled, Accepted, or Rejected.")
            .When(command => command.Request is not null);
        RuleFor(command => command.Request.RejectionReason)
            .MaximumLength(1000)
            .When(command => command.Request is not null
                && command.Request.Status == JobApplicationStatus.Rejected);
    }
}
