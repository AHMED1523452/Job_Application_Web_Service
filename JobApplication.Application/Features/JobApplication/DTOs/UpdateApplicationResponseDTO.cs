using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplication.DTOs
{
    public class UpdateApplicationResponseDTO
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string Description { get; set; }
    }
}
