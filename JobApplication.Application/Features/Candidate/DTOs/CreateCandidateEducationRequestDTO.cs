namespace JobApplication.Application.Features.Candidate.DTOs;

public sealed class CreateCandidateEducationRequestDTO
{
    public string InstitutionName { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? GradeOrGpa { get; set; }
    public string? Description { get; set; }
}
