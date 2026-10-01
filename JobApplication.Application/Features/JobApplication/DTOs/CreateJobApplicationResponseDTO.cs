using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplication.DTOs
{
    public class CreateJobApplicationResponseDTO
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string? JobTitle { get; set; }
        public Guid CandidateProfileId { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
