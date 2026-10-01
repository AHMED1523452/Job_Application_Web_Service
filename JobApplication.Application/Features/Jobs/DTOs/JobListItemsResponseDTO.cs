
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.Jobs.DTOs
{
    public class JobListItemsResponseDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string? CompanyLogoUrl { get; set; }
        public string CategoryName { get; set; } = null!;
        public EmploymentType EmploymentType { get; set; }
        public JobStatus JobStatus { get; set; }
        public string? Address { get; set; }
        public string Location { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? DistanceKm { get; set; }        // يظهر فقط عند البحث الجغرافي
        public DateTime? ApplicationDeadlineUtc { get; set; }
        public bool IsAcceptingApplications { get; set; }   // Open && (Deadline == null || Deadline > now)
        public DateTime CreatedAt { get; set; }
        public bool IsSavedByCurrentUser { get; set; }
    }
}
