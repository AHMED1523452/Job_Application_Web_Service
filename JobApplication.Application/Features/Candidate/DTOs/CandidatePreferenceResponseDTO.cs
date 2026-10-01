
namespace JobApplication.Application.Features.Candidate.DTOs
{
    public sealed class CandidatePreferenceResponseDTO
    {
        public Guid Id { get; set; }
        public decimal? MinimumMatchScore { get; set; }
        public string? PreferredLocation { get; set; }
        public bool RemoteOnly { get; set; }
        public int? MinimumYearsOfExperience { get; set; }
        public int? MaximumYearsOfExperience { get; set; }
        public bool NotificationEnabled { get; set; }
    }
}
