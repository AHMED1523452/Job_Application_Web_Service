using FluentValidation;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Features.Jobs.Handlers;
public sealed class JobSearchRequestDTOValidator : AbstractValidator<JobSearchRequestDTO>
{
    public JobSearchRequestDTOValidator(IUnitOfWork uow)
    {
        RuleFor(x=>x.Keyword).MaximumLength(150); RuleFor(x=>x.Location).MaximumLength(150);
        RuleFor(x=>x.JobCategoryId).MustAsync(async(id,ct)=>!id.HasValue||await uow.Repository<JobCategory>().AnyAsync(c=>c.Id==id.Value&&!c.IsDeleted&&!c.IsArchived,ct)).WithMessage("The selected job category does not exist.");
        RuleFor(x=>x.EmploymentType).IsInEnum().When(x=>x.EmploymentType.HasValue);
        RuleFor(x=>x.SortBy).Must(v=>v is "newest" or "relevance").WithMessage("SortBy must be 'newest' or 'relevance'. Salary sorting is unavailable because Jobs has no salary fields.");
        RuleFor(x=>x.Latitude).InclusiveBetween(-90,90).When(x=>x.Latitude.HasValue); RuleFor(x=>x.Longitude).InclusiveBetween(-180,180).When(x=>x.Longitude.HasValue); RuleFor(x=>x.RadiusKm).InclusiveBetween(25,200).When(x=>x.RadiusKm.HasValue);
        RuleFor(x=>x).Must(x=>(x.Latitude.HasValue&&x.Longitude.HasValue&&x.RadiusKm.HasValue)||(!x.Latitude.HasValue&&!x.Longitude.HasValue&&!x.RadiusKm.HasValue)).WithMessage("Latitude, longitude, and radius must be supplied together.");
        RuleFor(x=>x.SkillIds).Must(ids=>ids is null||(ids.Count<=50&&ids.All(id=>id!=Guid.Empty)&&ids.Distinct().Count()==ids.Count)).WithMessage("SkillIds must contain at most 50 unique, non-empty IDs.");
        RuleFor(x=>x.SkillIds).MustAsync(async(ids,ct)=>ids is null||ids.Count==0||(await uow.Repository<Skill>().GetAllAsync<Skill>(s=>ids.Contains(s.Id),cancellationToken:ct)).Count()==ids.Distinct().Count()).WithMessage("One or more selected skills do not exist.");
    }
}
