using JobApplication.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities{
    public class Skill : baseClass
    {
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public string NormalizedName { get; set; } = null!;
        public string? Description { get; set; }

        public long? SkillCategoryId { get; set; }

        [ForeignKey("SkillCategoryId")]
        public SkillCategory SkillCategory { get; set; }
        public ICollection<CandidateSkill> CandidateSkills { get; set; }
            = new List<CandidateSkill>();

    }
}