namespace JobApplication.Application.Features.Candidate.DTOs;

public sealed class CandidatePreferenceRequestDTO
{
    public decimal? MinimumMatchScore { get; set; }
    public string? PreferredLocation { get; set; }
    public bool RemoteOnly { get; set; }
    public int? MinimumYearsOfExperience { get; set; }
    public int? MaximumYearsOfExperience { get; set; }
    public bool NotificationEnabled { get; set; }
}
