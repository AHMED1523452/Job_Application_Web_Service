
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.Jobs.DTOs
{
    public class ChangeJobStatusRequestDTO
    {
        public JobStatus Status { get; set; }
    }
}
