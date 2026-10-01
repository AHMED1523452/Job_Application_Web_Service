using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.JobApplication.DTOs;

public sealed class ChangeJobApplicationStatusRequestDTO
{
    public JobApplicationStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}
