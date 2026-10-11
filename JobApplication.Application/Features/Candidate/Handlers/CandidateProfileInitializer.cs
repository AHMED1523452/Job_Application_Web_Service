using JobApplication.Domain.Entities;

namespace JobApplication.Application.Features.Candidate.Handlers;

internal static class CandidateProfileInitializer
{
    /// <summary>
    /// Iteration on each table that made a relation between Candidate 
    /// setting the Id and CandidateProfileId for each child entity and initializing audit fields
    /// and this method is called when creating a new CandidateProfile
    /// and that's why we need to set the Id and CandidateProfileId for each child entity
    /// 
    /// </summary>
    /// <param name="candidate"></param>
    /// <param name="actor"></param>
    public static void InitializeChildren(CandidateProfile candidate, string actor)
    {
        
        foreach (var skill in candidate.Skills)
        {
            skill.Id = $"{candidate.Id:N}:{skill.SkillId:N}";
            skill.CandidateProfileId = candidate.Id;
            InitializeAuditFields(skill, actor);
        }

        foreach (var experience in candidate.experiences)
        {
            experience.Id = Guid.NewGuid();
            experience.CandidateProfileId = candidate.Id;
            InitializeAuditFields(experience, actor);
        }

        foreach (var education in candidate.educations)
        {
            education.Id = Guid.NewGuid();
            education.CandidateProfileId = candidate.Id;
            InitializeAuditFields(education, actor);
        }

        foreach (var certification in candidate.certifications)
        {
            certification.Id = Guid.NewGuid();
            certification.CandidateProfileId = candidate.Id;
            InitializeAuditFields(certification, actor);
        }

        if (candidate.Preference is not null)
        {
            candidate.Preference.Id = Guid.NewGuid();
            candidate.Preference.CandidateProfileId = candidate.Id;
            InitializeAuditFields(candidate.Preference, actor);
        }
    }

    private static void InitializeAuditFields(baseClass entity, string actor)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = entity.CreatedAt;
        entity.CreatedBy = actor;
        entity.UpdatedBy = actor;
    }
}
