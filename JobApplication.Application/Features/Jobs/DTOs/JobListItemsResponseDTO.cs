
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.Jobs.DTOs
{
    public class JobListItemsResponseDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public HrProfielJobDetailsResponseDTO hrResponse { get; set; } = new();
        public JobCategoryResponseDTO categoryResponse { get; set; } = new();
        public EmploymentType EmploymentType { get; set; }
        public JobStatus JobStatus { get; set; }
        public string? Address { get; set; }
        public string Location { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? DistanceKm { get; set; }     
        public DateTime? ApplicationDeadlineUtc { get; set; }
        public bool IsAcceptingApplications { get; set; }   // Open && (Deadline == null || Deadline > now)
        public DateTime CreatedAt { get; set; }
        public bool IsSavedByCurrentUser { get; set; }
    }
}
