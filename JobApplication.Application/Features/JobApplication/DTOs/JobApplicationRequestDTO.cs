using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplication.DTOs
{
    public class JobApplicationRequestDTO
    {
        public Guid JobId { get; set; }
        public Guid CandidateCvId { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
