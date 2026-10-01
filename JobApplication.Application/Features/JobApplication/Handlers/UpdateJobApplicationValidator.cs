using FluentValidation;
using JobApplication.Application.Features.JobApplication.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplication.Handlers
{
    public class UpdateJobApplicationValidator : AbstractValidator<UpdateJobApplicationCommand>
    {
        public UpdateJobApplicationValidator()
        {
            RuleFor(x => x.candidateProfileId).NotEmpty();
            RuleFor(x => x.id).NotEmpty();
            RuleFor(x => x.requestDTO.Description)
                .NotEmpty()
                .WithMessage("Description is required.")
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters.");
        }
    }
}
