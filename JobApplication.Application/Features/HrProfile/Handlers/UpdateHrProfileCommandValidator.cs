using FluentValidation;
using JobApplication.Application.Features.HrProfile.Commands;

namespace JobApplication.Application.Features.HrProfile.Handlers;

public class UpdateHrProfileCommandValidator : AbstractValidator<UpdateHrProfileCommand>
{
    public UpdateHrProfileCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Name));

        RuleFor(x => x.Request.Address)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Address));

        RuleFor(x => x.Request.CompanyName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.CompanyName));

        RuleFor(x => x.Request.CompanyDescription)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.CompanyDescription));

        RuleFor(x => x.Request.CompanyWebsite)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.CompanyWebsite));

        RuleFor(x => x.Request.CompanyIndustry)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.CompanyIndustry));

        RuleFor(x => x.Request.CompanySize)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.CompanySize));

        RuleFor(x => x.Request.CompanyLocation)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.CompanyLocation));

        RuleFor(x => x.Request.CompanyType)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.CompanyType));
    }
}