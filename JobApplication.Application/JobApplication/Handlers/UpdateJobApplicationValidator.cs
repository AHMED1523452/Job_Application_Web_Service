using FluentValidation;
using JobApplication.Application.JobApplication.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.JobApplication.Handlers
{
    public class UpdateJobApplicationValidator : AbstractValidator<UpdateJobApplicationCommand>
    {
        public UpdateJobApplicationValidator()
        {
            RuleFor(x => x.requestDTO.Description)
                .NotEmpty()
                .WithMessage("Description is required.")
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters.");
        }
    }
}
