using AutoMapper;
using JobApplication.Application.Features.JobApplication.DTOs;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Mapping
{
    public class JobApplicationProfile : Profile
    {
        public JobApplicationProfile()
        {
            CreateMap<JobApplications, JobApplicationResponseDTO>()
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Job != null ? src.Job.Title : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            // mapping from request to entity handled in handlers, ignore audit fields
            // CreateMap<JobApplicationRequestDTO, JobApplications>();
        }
    }
}
