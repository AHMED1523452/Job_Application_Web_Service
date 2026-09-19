using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.JobApplication.DTOs
{
    public class CreateJobApplicationResponseDTO
    {
        public Guid JobId { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
