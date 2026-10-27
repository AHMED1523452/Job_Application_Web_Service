using FluentValidation;
using JobApplication.Application.Features.Candidate.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Candidate.Handlers
{
    public class UploadCandidateCvCommandValidator : AbstractValidator<UploadCandidateCvCommand>
    {
        public UploadCandidateCvCommandValidator(IValidatorService fileValidator)
        {
            RuleFor(x => x.Request).NotNull();

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request.FileStream)
                    .NotNull()
                    .Must(stream => stream is not null && stream != Stream.Null)
                    .WithMessage("A CV file is required.");

                RuleFor(x => x.Request.FileSize)
                    .GreaterThan(0)
                    .WithMessage("The CV file cannot be empty.");

                RuleFor(x => x.Request.FileName)
                    .NotEmpty()
                    .MaximumLength(255)
                    .WithMessage("A valid CV file name is required.");

                RuleFor(x => x.Request)
                    .Must(request => fileValidator.IsValidFileExtension(
                        request.FileName,
                        request.ContentType,
                        request.FileSize,
                        FileCategory.CandidateCV))
                    .WithMessage("The CV file type, content type, or size is not supported.");
            });
        }
    }
}
