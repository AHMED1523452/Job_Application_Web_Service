using FluentValidation;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Features.Candidate.Handlers;

public sealed class UpdateCandidateRequestDTOValidator : AbstractValidator<UpdateCandidateRequestDTO>
{
    public UpdateCandidateRequestDTOValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(5000);
        RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0).When(x => x.YearsOfExperience.HasValue);
        RuleFor(x => x.Location).MaximumLength(250);
        RuleFor(x => x.ProfileImageId)
            .MustAsync(async (id, ct) => !id.HasValue || await unitOfWork.Repository<MediaFile>().AnyAsync(file => file.MediaFileID == id.Value && !file.IsDeleted, ct))
            .WithMessage("The selected profile image does not exist.");

        RuleFor(x => x.Skills).Must(items => items is not null && items.Count <= 100)
            .WithMessage("A candidate can have at most 100 skills.");
        RuleFor(x => x.Skills).Must(items => items is not null && items.Select(item => item.SkillId).Distinct().Count() == items.Count)
            .WithMessage("A skill can only be listed once.");
        RuleForEach(x => x.Skills).ChildRules(skill =>
        {
            skill.RuleFor(x => x.SkillId).NotEmpty()
                .MustAsync((id, ct) => unitOfWork.Repository<Skill>().AnyAsync(item => item.Id == id, ct))
                .WithMessage("The selected skill does not exist.");
            skill.RuleFor(x => x.ProficiencyLevel).IsInEnum();
            skill.RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0).When(x => x.YearsOfExperience.HasValue);
        });

        RuleFor(x => x.Experiences).Must(items => items is not null && items.Count <= 50)
            .WithMessage("A candidate can have at most 50 experience entries.");
        RuleFor(x => x.Experiences).Must(items => items is not null
            && items.Where(item => item.Id.HasValue).Select(item => item.Id).Distinct().Count() == items.Count(item => item.Id.HasValue))
            .WithMessage("Experience IDs must be unique within the request.");
        RuleForEach(x => x.Experiences).Must(item => !item.Id.HasValue || item.Id.Value != Guid.Empty)
            .WithMessage("Experience ID cannot be empty.");
        RuleForEach(x => x.Experiences).ChildRules(entry =>
        {
            entry.RuleFor(x => x.JobTitle).NotEmpty().MaximumLength(150);
            entry.RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(150);
            entry.RuleFor(x => x.Location).MaximumLength(250);
            entry.RuleFor(x => x.Description).MaximumLength(3000);
            entry.RuleFor(x => x.StartDate).Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Start date cannot be in the future.");
            entry.RuleFor(x => x.EndDate).Must((item, endDate) => !endDate.HasValue || endDate.Value >= item.StartDate)
                .WithMessage("End date must be on or after the start date.");
            entry.RuleFor(x => x.EndDate).Must((item, endDate) => !item.IsCurrent || !endDate.HasValue)
                .WithMessage("A current job must not have an end date.");
        });

        RuleFor(x => x.Educations).Must(items => items is not null && items.Count <= 50)
            .WithMessage("A candidate can have at most 50 education entries.");
        RuleFor(x => x.Educations).Must(items => items is not null
            && items.Where(item => item.Id.HasValue).Select(item => item.Id).Distinct().Count() == items.Count(item => item.Id.HasValue))
            .WithMessage("Education IDs must be unique within the request.");
        RuleForEach(x => x.Educations).Must(item => !item.Id.HasValue || item.Id.Value != Guid.Empty)
            .WithMessage("Education ID cannot be empty.");
        RuleForEach(x => x.Educations).ChildRules(entry =>
        {
            entry.RuleFor(x => x.InstitutionName).NotEmpty().MaximumLength(200);
            entry.RuleFor(x => x.Degree).NotEmpty().MaximumLength(150);
            entry.RuleFor(x => x.FieldOfStudy).MaximumLength(150);
            entry.RuleFor(x => x.GradeOrGpa).MaximumLength(100);
            entry.RuleFor(x => x.Description).MaximumLength(3000);
            entry.RuleFor(x => x.StartDate).Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Start date cannot be in the future.");
            entry.RuleFor(x => x.EndDate).Must((item, endDate) => !endDate.HasValue || endDate.Value >= item.StartDate)
                .WithMessage("End date must be on or after the start date.");
        });

        RuleFor(x => x.Certifications).Must(items => items is not null && items.Count <= 50)
            .WithMessage("A candidate can have at most 50 certifications.");
        RuleFor(x => x.Certifications).Must(items => items is not null
            && items.Where(item => item.Id.HasValue).Select(item => item.Id).Distinct().Count() == items.Count(item => item.Id.HasValue))
            .WithMessage("Certification IDs must be unique within the request.");
        RuleForEach(x => x.Certifications).Must(item => !item.Id.HasValue || item.Id.Value != Guid.Empty)
            .WithMessage("Certification ID cannot be empty.");
        RuleForEach(x => x.Certifications).ChildRules(entry =>
        {
            entry.RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            entry.RuleFor(x => x.IssuingOrganization).NotEmpty().MaximumLength(200);
            entry.RuleFor(x => x.CredentialId).MaximumLength(200);
            entry.RuleFor(x => x.IssueDate).Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Issue date cannot be in the future.");
            entry.RuleFor(x => x.CredentialUrl).Must(value => string.IsNullOrWhiteSpace(value) || Uri.TryCreate(value, UriKind.Absolute, out _))
                .WithMessage("Credential URL must be a valid absolute URL.");
            entry.RuleFor(x => x.ExpirationDate).Must((item, expiry) => !expiry.HasValue || expiry.Value >= item.IssueDate)
                .WithMessage("Expiration date must be on or after the issue date.");
        });

        When(x => x.Preference is not null, () =>
        {
            RuleFor(x => x.Preference!.MinimumMatchScore).InclusiveBetween(0, 100)
                .When(x => x.Preference!.MinimumMatchScore.HasValue);

            RuleFor(x => x.Preference!.PreferredLocation)
                        .MaximumLength(250);
            RuleFor(x => x.Preference!.MinimumYearsOfExperience)
                         .GreaterThanOrEqualTo(0)
                .When(x => x.Preference!.MinimumYearsOfExperience.HasValue);
            RuleFor(x => x.Preference!.MaximumYearsOfExperience)
                          .GreaterThanOrEqualTo(0)
                .When(x => x.Preference!.MaximumYearsOfExperience.HasValue);
            RuleFor(x => x.Preference).Must(p => !p!.MinimumYearsOfExperience.HasValue || !p.MaximumYearsOfExperience.HasValue
                || p.MinimumYearsOfExperience <= p.MaximumYearsOfExperience)
                .WithMessage("Minimum experience cannot exceed maximum experience.");
        });
    }
}
