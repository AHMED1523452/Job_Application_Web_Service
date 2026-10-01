using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplication.DTOs
{
    public class CloseJobApplicationResponseDTO
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
    }
}
