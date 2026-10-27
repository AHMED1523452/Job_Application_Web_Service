namespace JobApplication.Application.Features.Candidate.DTOs;

public sealed class CandidateProfileImageUploadDTO
{
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
}
