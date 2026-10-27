using FluentValidation;
using JobApplication.Application.Features.HrProfile.Commands;

namespace JobApplication.Application.Features.HrProfile.Handlers
{
    public class UploadCompanyLogoCommandValidator : AbstractValidator<UploadCompanyLogoCommand>
    {
        public UploadCompanyLogoCommandValidator()
        {
            RuleFor(x => x.LogoFile)
                .NotNull().WithMessage("Logo file is required.")
                .Must(f => f.Length > 0).WithMessage("Logo file cannot be empty.");
        }
    }
}
