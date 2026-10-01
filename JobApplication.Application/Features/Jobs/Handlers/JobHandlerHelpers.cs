using System.Globalization;
using AutoMapper;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using NetTopologySuite.Geometries;

using JobEntity = JobApplication.Domain.Entities.Jobs;
namespace JobApplication.Application.Features.Jobs.Handlers;

internal static class JobHandlerHelpers
{
    public static Point? MakePoint(double? latitude, double? longitude) =>
        latitude.HasValue && longitude.HasValue
            ? new Point(longitude.Value, latitude.Value) { SRID = 4326 }
            : null;

    public static async Task<List<JobRequirements>> MapRequirementsAsync(
        IUnitOfWork unitOfWork, Guid jobId, IEnumerable<JobRequirementDTO> requirements, CancellationToken ct)
    {
        var result = new List<JobRequirements>();
        foreach (var item in requirements)
        {
            var skill = item.SkillId.HasValue
                ? await unitOfWork.Repository<Skill>().GetByIdAsync(
                    x => x.Id == item.SkillId.Value, ct)
                : null;
            var years = item.MinYearsExperience?.ToString(CultureInfo.InvariantCulture);
            result.Add(new JobRequirements
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                SkillId = item.SkillId,
                RequirementType = item.SkillId.HasValue ? RequirementTypes.Skill : RequirementTypes.Experience,
                Name = skill?.Name ?? item.Description ?? "Experience",
                Description = item.Description,
                IsRequired = item.IsRequired,
                Weight = 1m,
                ExpectedValue = years
            });
        }
        return result;
    }

    public static async Task<JobDetailsResponseDTO> ToDetailsAsync(
        IUnitOfWork unitOfWork, IMapper mapper, JobEntity job, Guid? currentUserId, CancellationToken ct)
    {
        var dto = mapper.Map<JobDetailsResponseDTO>(job);
        dto.Requirements = mapper.Map<List<JobRequirementResponse>>(job.JobRequirements ?? new List<JobRequirements>());
        dto.ViewCount = (await unitOfWork.Repository<JobView>().GetAllAsync<JobView>(x => x.JobId == job.Id, cancellationToken: ct)).Count();
        dto.ApplicationCount = (await unitOfWork.Repository<JobApplications>().GetAllAsync<JobApplications>(x => x.JobId == job.Id, cancellationToken: ct)).Count();
        if (currentUserId.HasValue)
        {
            var profile = await unitOfWork.Repository<CandidateProfile>().GetByIdAsync(x => x.UserId == currentUserId.Value, ct);
            if (profile is not null)
                dto.IsSavedByCurrentUser = await unitOfWork.Repository<SavedJobs>().AnyAsync(
                    x => x.JobId == job.Id && x.CandidateProfileId == profile.Id, ct);
        }
        return dto;
    }

    public static async Task<List<JobListItemsResponseDTO>> ToListAsync(
        IUnitOfWork unitOfWork, IMapper mapper, IEnumerable<JobEntity> jobs, Guid? currentUserId, CancellationToken ct)
    {
        var items = mapper.Map<List<JobListItemsResponseDTO>>(jobs);
        if (!currentUserId.HasValue) return items;
        var profile = await unitOfWork.Repository<CandidateProfile>().GetByIdAsync(x => x.UserId == currentUserId.Value, ct);
        if (profile is null) return items;
        var savedIds = (await unitOfWork.Repository<SavedJobs>().GetAllAsync<SavedJobs>(
            x => x.CandidateProfileId == profile.Id, cancellationToken: ct)).Select(x => x.JobId).ToHashSet();
        foreach (var item in items) item.IsSavedByCurrentUser = savedIds.Contains(item.Id);
        return items;
    }

    public static async Task<(IEnumerable<JobEntity> Items, int TotalCount)> GetJobPageAsync(
        IUnitOfWork unitOfWork, int page, int pageSize, System.Linq.Expressions.Expression<Func<JobEntity, bool>> predicate,
        string? searchTerm, string? orderBy, CancellationToken ct) =>
        await unitOfWork.Repository<JobEntity>().GetPagedAsync<JobEntity>(page, pageSize, predicate, x => x,
            orderBy, orderByDescending: true, searchTerm: searchTerm, cancellationToken: ct,
            x => x.HrProfile, x => x.JobCategory, x => x.JobRequirements);
}
