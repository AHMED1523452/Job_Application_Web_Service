using System.ComponentModel.DataAnnotations;

namespace JobApplication.Domain.Entities{
    public class CandidateSkill : baseClass
    {
        //. The Primary key must be the CandidateProfileId +  Skill Id 
         [Key] 
         public string Id { get; set; }
         public Guid CandidateProfileId { get; set; }
         public CandidateProfile CandidateProfile { get; set; } = null!;

         public Guid SkillId { get; set; }
         public Skill Skill { get; set; } = null!;

         public SkillProficiencyLevel ProficiencyLevel { get; set; }

         public int? YearsOfExperience { get; set; }
    
    }
}