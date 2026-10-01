using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobApplication.DTOs
{
    public class CancelJobApplicationResponse()
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
    };
}
