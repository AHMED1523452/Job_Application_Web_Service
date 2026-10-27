using FluentValidation;
using JobApplication.Application.Features.JobApplication.Commands;

namespace JobApplication.Application.Features.JobApplication.Handlers
{
    public class CreateJobApplicationValidator : AbstractValidator<CreateJobApplicationCommand>
    {
        public CreateJobApplicationValidator()
        {
            RuleFor(x => x.candidateProfileId)
                .NotEmpty()
                .WithMessage("Candidate profile ID is required.");

            RuleFor(x => x.requestDTO.JobId)
                .NotEmpty()
                .WithMessage("Job ID is required.");

            RuleFor(x => x.requestDTO.CandidateCvId)
                .NotEmpty()
                .WithMessage("Candidate CV ID is required.");

            RuleFor(x => x.requestDTO.Description)
                .NotEmpty()
                .WithMessage("Description is required.")
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters.");
        }
    }

}
