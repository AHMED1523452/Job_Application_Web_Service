using JobApplication.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities{
    public class JobRequirements : baseClass
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid JobId { get; set; }

        public Jobs Job { get; set; } = null!;

        public Guid? SkillId { get; set; }
        public Skill? Skill { get; set; }

        public RequirementTypes RequirementType { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsRequired { get; set; }

        public decimal Weight { get; set; }

        public string? ExpectedValue { get; set; }

        public ICollection<RequirementMatch> RequirementMatches { get; set; }
            = new List<RequirementMatch>();
    }
}
