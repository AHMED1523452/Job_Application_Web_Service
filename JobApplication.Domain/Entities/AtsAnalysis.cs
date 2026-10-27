using System.ComponentModel.DataAnnotations.Schema;
using JobApplication.Domain.Enums;

namespace JobApplication.Domain.Entities{
    public class AtsAnalysis : baseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid JobApplicationId { get; set; }
        public JobApplications JobApplication { get; set; } = null!;

        public decimal AtsScore { get; set; }

        public decimal RequirementsScore { get; set; }

        public decimal FinalScore { get; set; }

        /// <summary>
        /// default value --> pending 
        /// after Analysis --> Recommended or not recommended
        /// </summary>
        public AtsDescision Decision { get; set; }

        public int TotalRequirementsCount { get; set; }

        public int MatchedRequirementsCount { get; set; }

        public int MissingRequirementsCount { get; set; }

        public DateTime AnalyzedAtUtc { get; set; }

        public ICollection<RequirementMatch> RequirementMatches { get; set; }
            = new List<RequirementMatch>();
    }
}