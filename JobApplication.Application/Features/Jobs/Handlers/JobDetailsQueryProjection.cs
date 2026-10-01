using System.Linq.Expressions;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using JobEntity = JobApplication.Domain.Entities.Jobs;

namespace JobApplication.Application.Features.Jobs.Handlers;

internal sealed class JobDetailsQueryProjection
{
    public Guid RecruiterUserId { get; init; }
    public JobDetailsResponseDTO Details { get; init; } = null!;
    public List<JobRequirementQueryProjection> Requirements { get; init; } = [];

    public static Expression<Func<JobEntity, JobDetailsQueryProjection>> Select { get; } = job => new JobDetailsQueryProjection
    {
        RecruiterUserId = job.HrProfile.UserId,
        Details = new JobDetailsResponseDTO
        {
            Id = job.Id,
            Title = job.Title,
            CompanyName = job.HrProfile.CompanyName,
            CompanyLogoUrl = job.HrProfile.CompanyLogo,
            CategoryName = job.JobCategory.Name,
            EmploymentType = job.employmentType,
            JobStatus = job.jobStatus,
            Address = job.Address,
            Location = job.Location,
            Latitude = job.Latitude,
            Longitude = job.Longitude,
            ApplicationDeadlineUtc = job.ApplicationDeadlineUtc,
            IsAcceptingApplications = job.jobStatus == JobStatus.Published
                && (!job.ApplicationDeadlineUtc.HasValue || job.ApplicationDeadlineUtc > DateTime.UtcNow),
            CreatedAt = job.CreatedAt,
            Description = job.Description
        },
        Requirements = job.JobRequirements
            .Where(requirement => requirement.JobId == job.Id
                && requirement.IsActive
                && !requirement.IsArchived
                && !requirement.IsDeleted)
            .Select(requirement => new JobRequirementQueryProjection
            {
                Id = requirement.Id,
                SkillId = requirement.SkillId,
                SkillName = requirement.Skill != null ? requirement.Skill.Name : requirement.Name,
                Description = requirement.Description,
                IsRequired = requirement.IsRequired,
                ExpectedValue = requirement.ExpectedValue
            }).ToList()
    };
}

internal sealed class JobRequirementQueryProjection
{
    public Guid Id { get; init; }
    public Guid? SkillId { get; init; }
    public string? SkillName { get; init; }
    public string? Description { get; init; }
    public bool IsRequired { get; init; }
    public string? ExpectedValue { get; init; }
}
