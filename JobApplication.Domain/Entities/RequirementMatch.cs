using JobApplication.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities{
    public class RequirementMatch : baseClass{
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid AtsAnalysisId { get; set; }
        public AtsAnalysis AtsAnalysis { get; set; } = null!;

        public Guid JobRequirementId { get; set; }
        public JobRequirements JobRequirement { get; set; } = null!;

        public MatchResult Result { get; set; }

        public decimal Score { get; set; }

        public string? Evidence { get; set; }

        public string? Reason { get; set; }
    }
}