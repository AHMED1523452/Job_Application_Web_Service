using FluentValidation;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.Jobs.Handlers;
public sealed class JobRequirementDTOValidator : AbstractValidator<JobRequirementDTO>
{
    public JobRequirementDTOValidator(IUnitOfWork uow)
    {
        RuleFor(x=>x.SkillId).MustAsync(async(id,ct)=>!id.HasValue||await uow.Repository<Skill>().AnyAsync(s=>s.Id==id.Value,ct)).WithMessage("The selected skill does not exist.");
        RuleFor(x=>x.Description).MaximumLength(2000);
        RuleFor(x=>x.MinYearsExperience).GreaterThanOrEqualTo(0).When(x=>x.MinYearsExperience.HasValue);
        RuleFor(x=>x).Must(x=>x.SkillId.HasValue||x.MinYearsExperience.HasValue||!string.IsNullOrWhiteSpace(x.Description)).WithMessage("Each requirement must include a skill, experience minimum, or description.");
    }
}
public sealed class JobRequestDTOValidator : AbstractValidator<JobRequestDTO>
{
    public JobRequestDTOValidator(IUnitOfWork uow)
    {
        RuleFor(x=>x.Title).NotEmpty().MaximumLength(100); RuleFor(x=>x.Description).MaximumLength(10000);
        RuleFor(x=>x.JobCategoryId).NotEmpty().MustAsync((id,ct)=>uow.Repository<JobCategory>().AnyAsync(c=>c.Id==id&&!c.IsDeleted&&!c.IsArchived,ct)).WithMessage("Select an existing active job category.");
        RuleFor(x=>x.EmploymentType).IsInEnum();
        RuleFor(x=>x.InitialStatus).Must(s=>s is JobStatus.Draft or JobStatus.Published).WithMessage("A new job can only start as Draft or Published.");
        RuleFor(x=>x.Address).MaximumLength(300); RuleFor(x=>x.Location).MaximumLength(300);
        RuleFor(x=>x.Latitude).InclusiveBetween(-90,90).When(x=>x.Latitude.HasValue); RuleFor(x=>x.Longitude).InclusiveBetween(-180,180).When(x=>x.Longitude.HasValue);
        RuleFor(x=>x).Must(x=>x.Latitude.HasValue==x.Longitude.HasValue).WithMessage("Latitude and longitude must be supplied together.");
        RuleFor(x=>x.ApplicationDeadlineUtc).Must(d=>d>DateTime.UtcNow).When(x=>x.ApplicationDeadlineUtc.HasValue).WithMessage("Application deadline must be in the future.");
        RuleFor(x=>x.Requirements).Must(r=>r is not null&&r.Count<=50).WithMessage("A job can have at most 50 requirements.");
        RuleForEach(x=>x.Requirements).SetValidator(new JobRequirementDTOValidator(uow));
    }
}
