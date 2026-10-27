using AutoMapper;
using FluentValidation;
using JobApplication.Application.Features.Candidate.Commands;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
namespace JobApplication.Application.Features.Candidate.Handlers
{
    public class CreateCandidateCommandValidator : AbstractValidator<CreateCandidateCommand>
    {
        public CreateCandidateCommandValidator(IUnitOfWork unitOfWork, IMapper mapper, IValidatorService fileValidator)
        {
            var updateShapeValidator = new UpdateCandidateRequestDTOValidator(unitOfWork);

            RuleFor(x => x.Request).NotNull();
            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request).CustomAsync(async (request, context, cancellationToken) =>
                {
                    UpdateCandidateRequestDTO updateShape = mapper.Map<UpdateCandidateRequestDTO>(request);
                    var validation = await updateShapeValidator.ValidateAsync(updateShape, cancellationToken);
                    foreach (var failure in validation.Errors)
                        context.AddFailure(failure.PropertyName, failure.ErrorMessage);
                });
            });

            When(x => x.ProfileImage is not null, () =>
            {
                RuleFor(x => x.ProfileImage!.FileStream)
                    .NotNull()
                    .Must(stream => stream is not null && stream != Stream.Null)
                    .WithMessage("The profile image file is required.");
                RuleFor(x => x.ProfileImage!.FileName)
                    .NotEmpty()
                    .MaximumLength(255)
                    .WithMessage("A valid profile image filename is required.");
                RuleFor(x => x.ProfileImage!.FileSize)
                    .GreaterThan(0)
                    .WithMessage("The profile image cannot be empty.");
                RuleFor(x => x.ProfileImage!)
                    .Must(image => fileValidator.IsValidFileExtension(
                        image.FileName,
                        image.ContentType,
                        image.FileSize,
                        FileCategory.ProfileImage))
                    .WithMessage("The profile image type or size is not supported.");
            });
        }
    }
}
