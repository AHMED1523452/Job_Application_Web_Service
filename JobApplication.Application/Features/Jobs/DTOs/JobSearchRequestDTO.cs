using JobApplication.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.Features.Jobs.DTOs
{
    public class JobSearchRequestDTO
    {
        public string? Keyword { get; set; }
        public Guid? JobCategoryId { get; set; }
        public EmploymentType? EmploymentType { get; set; }
        public string? Location { get; set; }        
        public List<Guid>? SkillIds { get; set; }

        // بحث جغرافي (الثلاثة معاً أو لا شيء)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        [Range(25,200)]
        public double? RadiusKm { get; set; }          

        public string SortBy { get; set; } = "newest";   // newest | relevance; Jobs has no salary fields yet.
    }
}
