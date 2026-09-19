using FluentValidation;
using JobApplication.Application.JobApplication.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.JobApplication.Handlers
{
    public class CreateJobApplicationValidator : AbstractValidator<CreateJobApplicationCommand>
    {
        public CreateJobApplicationValidator()
        {
            RuleFor(x => x.requestDTO.Id)
                .NotEmpty()
                .WithMessage("JobId is required.");

            RuleFor(x => x.requestDTO.Description)
                .NotEmpty()
                .WithMessage("Description is required.")
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters.");
        }
    }
}
