using System;
using AutoMapper.Configuration.Annotations;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.Jobs.DTOs
{
    public class JobRequestDTO
    {
        public string Title { get; set; } = null!;     
        public string? Description { get; set; }
        public Guid JobCategoryId { get; set; }
        public EmploymentType EmploymentType { get; set; }

        // الموقع
        public string? Address { get; set; }                  
        public string Location { get; set; } = string.Empty;  
        public double? Latitude { get; set; }         
        public double? Longitude { get; set; }          

        public DateTime? ApplicationDeadlineUtc { get; set; }
        public JobStatus InitialStatus { get; set; } = JobStatus.Draft;  
        public List<JobRequirementDTO> Requirements { get; set; } = new();

    }
}