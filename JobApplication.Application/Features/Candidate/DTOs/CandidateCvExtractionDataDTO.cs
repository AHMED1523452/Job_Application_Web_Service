namespace JobApplication.Application.Features.Candidate.DTOs
{
    public sealed class CandidateCvExtractionDataDTO
    {
        public string StoragePath { get; init; } = string.Empty;
        public string OriginalFileName { get; init; } = string.Empty;
    }
}
