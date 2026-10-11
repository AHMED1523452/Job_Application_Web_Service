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

            CreateMap<JobApplicationRequestDTO, JobApplications>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.JobId))
                .ForMember(dest => dest.CandidateCvId, opt => opt.MapFrom(src => src.CandidateCvId))
                .ForMember(dest => dest.CoverLetter, opt => opt.MapFrom(src => src.Description.Trim()))
                .ForMember(dest => dest.CandidateProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateCv, opt => opt.Ignore())
                .ForMember(dest => dest.Job, opt => opt.Ignore());

            // mapping from request to entity handled in handlers, ignore audit fields
            // CreateMap<JobApplicationRequestDTO, JobApplications>();
        }
    }
}
