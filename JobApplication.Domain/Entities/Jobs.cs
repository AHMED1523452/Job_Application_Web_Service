using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobApplication.Domain.Enums;

// System.Drawing removed to avoid ambiguity with NetTopologySuite.Geometries.Point
using NetTopologySuite.Geometries;

namespace JobApplication.Domain.Entities
{
    public class Jobs : baseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Title is required"),
                MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }
        public string? Description { get; set; }
        // Human-readable address or place name
        public string? Address { get; set; }
        // Optional legacy location string (kept for backward compatibility)
        public string Location { get; set; } = string.Empty;
        // Optional numeric coordinates for simple geo use-cases
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public DateTime? ApplicationDeadlineUtc { get; set; }
        // Spatial point for accurate geospatial queries (NetTopologySuite)
        public Point? LocationPoint { get; set; }

        public EmploymentType employmentType { get; set; }
        public JobStatus jobStatus { get; set; }

        public Guid HrProfileId { get; set; }
        public HrProfile HrProfile { get; set; } = null!;

        public Guid JobCategoryId { get; set; }
        public JobCategory JobCategory { get; set; } = null!;

        public ICollection<JobRecommendation> JobRecommendations { get; set; }
        = new List<JobRecommendation>();
        public ICollection<JobRequirements> JobRequirements { get; set; }
        = new List<JobRequirements>();

        public ICollection<CandidateRecommendation> CandidateRecommendations { get; set; }
            = new List<CandidateRecommendation>();
        public ICollection<SavedJobs> SavedByCandidates { get; set; } = new List<SavedJobs>();
        public ICollection<JobView> Views { get; set; } = new List<JobView>();
        public ICollection<MatchingHistory> MatchingHistories { get; set; } = new List<MatchingHistory>();
        
        public ICollection<JobApplications> jobApplications { get; set; } = new List<JobApplications>();
    }
}
