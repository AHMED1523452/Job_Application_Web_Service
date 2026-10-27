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

    //. mapping for list of job requirements and adding it in the job requirements table
    public static async Task<List<JobRequirements>> MapRequirementsAsync(
                                                                IUnitOfWork unitOfWork, 
                                                                Guid jobId, 
                                                                IEnumerable<JobRequirementDTO> requirements,
                                                                CancellationToken ct)
    {
        var result = new List<JobRequirements>();
        //. working in cpu
        foreach (var item in requirements)
        {
            var skill = item.SkillId.HasValue
                ? await unitOfWork.Repository<Skill>().GetByIdAsyncWithTracking(
                    x => x.Id == item.SkillId.Value, ct)
                : null;

            var years = item.MinYearsExperience?.ToString(CultureInfo.InvariantCulture);
            //. the whole work will be in memory 
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
                ExpectedValue = years //. as minimum 
            });
        }
        return result;
    }

    //. there is another solution
    public static async Task<JobDetailsResponseDTO> ToDetailsAsync(
                                        IUnitOfWork unitOfWork, 
                                        IMapper mapper,
                                        JobEntity job,
                                        Guid? currentUserId,
                                        CancellationToken ct)
    {
        var dto = mapper.Map<JobDetailsResponseDTO>(job);

        dto.Requirements = mapper.Map<List<JobRequirementResponse>>(job.JobRequirements ?? new List<JobRequirements>());
        dto.ViewCount = (await unitOfWork.Repository<JobView>()
                    .GetAllAsync<JobView>(x => x.JobId == job.Id, cancellationToken: ct)).Count();

        //. we will count the applications is not deleted and not archieved and active and not tracking (what a fuck is that ?)
        dto.ApplicationCount = (await unitOfWork.Repository<JobApplications>()
                    .GetAllAsync<JobApplications>(x => x.JobId == job.Id &&
                                             !x.IsDeleted &&
                                             !x.IsArchived && 
                                              x.IsActive, cancellationToken: ct)).Count();
        if (currentUserId.HasValue)
        {
            Guid profile = await unitOfWork.Repository<CandidateProfile>().GetByIdAsyncWithTracking(x =>
                                                            x.UserId == currentUserId.Value && 
                                                            !x.IsDeleted&& 
                                                            !x.IsArchived && 
                                                            x.IsActive,
                                                            selector: op => op.Id ,ct);
            if (profile == default) //. is the current user has a candidate profile or not (the current user must be recruiter or admin 
                dto.IsSavedByCurrentUser = await unitOfWork.Repository<SavedJobs>().AnyAsync(
                    x => x.JobId == job.Id &&
                    x.CandidateProfileId == profile
                    , ct);
        }
        return dto;
    }

    public static async Task<List<JobListItemsResponseDTO>> ToListAsync(
                                                             IUnitOfWork unitOfWork, 
                                                             IMapper mapper, 
                                                             IEnumerable<JobEntity> jobs,
                                                             Guid? currentUserId,
                                                             CancellationToken ct)
    {
        List<JobListItemsResponseDTO> items = mapper.Map<List<JobListItemsResponseDTO>>(jobs);
        if (!currentUserId.HasValue) 
            return items;

        Guid? profile = await unitOfWork.Repository<CandidateProfile>()
                            .GetByIdAsyncWithTracking(x => x.UserId == currentUserId.Value && 
                                                           !x.IsDeleted && 
                                                           !x.IsArchived && 
                                                           x.IsActive,
                                                           selector: op => op.Id ,
                                                           ct);
        if (profile is null) 
            return items;

        HashSet<Guid> savedIds = (await unitOfWork.Repository<SavedJobs>().GetAllAsync<SavedJobs>(
                                     x => x.CandidateProfileId == profile,
                                     cancellationToken: ct))
                                     .Select(x => x.JobId)
                                     .ToHashSet(); //. more better for search process

        foreach (var item in items) 
            item.IsSavedByCurrentUser = savedIds.Contains(item.Id);
        return items;
    }
}
