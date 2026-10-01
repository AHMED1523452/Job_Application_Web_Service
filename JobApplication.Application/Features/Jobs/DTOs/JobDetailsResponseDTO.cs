
namespace JobApplication.Application.Features.Jobs.DTOs
{
    public class JobDetailsResponseDTO : JobListItemsResponseDTO
    {
        public string? Description { get; set; }
        public int ViewCount { get; set; }
        public int ApplicationCount { get; set; }
        public List<JobRequirementResponse> Requirements { get; set; } = new();
    }
}
