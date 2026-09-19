using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.JobApplication.DTOs
{
    public class JobApplicationRequestDTO
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
