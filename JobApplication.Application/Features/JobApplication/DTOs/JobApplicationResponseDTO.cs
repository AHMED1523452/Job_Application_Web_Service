using System;

namespace JobApplication.Application.Features.JobApplication.DTOs
{
    public class JobApplicationResponseDTO
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string? JobTitle { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid CandidateId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
