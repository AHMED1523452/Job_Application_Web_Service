using System.Linq.Expressions;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Features.Candidate.Handlers;

internal static class CandidateProfileResponseSelector
{
    public static Expression<Func<CandidateProfile, CandidateResponseDTO>> Select { get; } = candidate => new CandidateResponseDTO
    {
        Id = candidate.Id,
        UserId = candidate.UserId,
        Title = candidate.Title ?? string.Empty,
        Description = candidate.Description,
        YearsOfExperience = candidate.YearsOfExperience,
        Location = candidate.Location,
        ProfileImageId = candidate.ProfileImageId,
        Skills = candidate.Skills
            .Where(skill => skill.CandidateProfileId == candidate.Id && skill.IsActive && !skill.IsArchived && !skill.IsDeleted)
            .Select(skill => new CandidateSkillResponseDTO
            {
                Id = skill.Id,
                SkillId = skill.SkillId,
                SkillName = skill.Skill.Name,
                ProficiencyLevel = skill.ProficiencyLevel,
                YearsOfExperience = skill.YearsOfExperience
            }).ToList(),
        Experiences = candidate.experiences
            .Where(experience => experience.CandidateProfileId == candidate.Id && experience.IsActive && !experience.IsArchived && !experience.IsDeleted)
            .Select(experience => new CandidateExperienceResponseDTO
            {
                Id = experience.Id,
                JobTitle = experience.JobTitle,
                CompanyName = experience.CompanyName,
                Location = experience.Location,
                StartDate = experience.StartDate,
                EndDate = experience.EndDate,
                IsCurrent = experience.IsCurrent,
                Description = experience.Description
            }).ToList(),
        Educations = candidate.educations
            .Where(education => education.CandidateProfileId == candidate.Id && education.IsActive && !education.IsArchived && !education.IsDeleted)
            .Select(education => new CandidateEducationResponseDTO
            {
                Id = education.Id,
                InstitutionName = education.InstitutionName,
                Degree = education.Degree,
                FieldOfStudy = education.FieldOfStudy,
                StartDate = education.StartDate,
                EndDate = education.EndDate,
                GradeOrGpa = education.GradeOrGpa,
                Description = education.Description
            }).ToList(),
        Certifications = candidate.certifications
            .Where(certification => certification.CandidateProfileId == candidate.Id && certification.IsActive && !certification.IsArchived && !certification.IsDeleted)
            .Select(certification => new CandidateCertificationResponseDTO
            {
                Id = certification.Id,
                Name = certification.Name,
                IssuingOrganization = certification.IssuingOrganization,
                IssueDate = certification.IssueDate,
                ExpirationDate = certification.ExpirationDate,
                CredentialId = certification.CredentialId,
                CredentialUrl = certification.CredentialUrl
            }).ToList(),
        CVs = candidate.CVs
            .Where(cv => cv.CandidateProfileId == candidate.Id)
            .Select(cv => new CandidateCvResponseDTO
            {
                Id = cv.Id,
                OriginalFileName = cv.OriginalFileName,
                ContentType = cv.ContentType,
                FileSize = cv.FileSize,
                ProcessingStatus = cv.ProcessingStatus,
                UploadedAtUtc = cv.UploadedAtUtc,
                ProcessedAtUtc = cv.ProcessedAtUtc
            }).ToList(),
        preference = candidate.Preference == null
            ? null
            : new CandidatePreferenceResponseDTO
            {
                Id = candidate.Preference.Id,
                MinimumMatchScore = candidate.Preference.MinimumMatchScore,
                PreferredLocation = candidate.Preference.PreferredLocation,
                RemoteOnly = candidate.Preference.RemoteOnly,
                MinimumYearsOfExperience = candidate.Preference.MinimumYearsOfExperience,
                MaximumYearsOfExperience = candidate.Preference.MaximumYearsOfExperience,
                NotificationEnabled = candidate.Preference.NotificationEnabled
            }
    };
}
