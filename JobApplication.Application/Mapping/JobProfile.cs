using AutoMapper;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Mapping
{
    public class JobProfile : Profile
    {
        public JobProfile()
        {
            CreateMap<Jobs, JobListItemsResponseDTO>()
                .ForMember(dest => dest.hrResponse.CompanyName, opt => opt.MapFrom(src => src.HrProfile.CompanyName))
                .ForMember(dest => dest.hrResponse.CompanyLogoURL, opt => opt.MapFrom(src => src.HrProfile.CompanyLogoPath))
                .ForMember(dest => dest.categoryResponse.CategoryName, opt => opt.MapFrom(src => src.JobCategory.Name))
                .ForMember(dest => dest.EmploymentType, opt => opt.MapFrom(src => src.employmentType))
                .ForMember(dest => dest.JobStatus, opt => opt.MapFrom(src => src.jobStatus))
                .ForMember(dest => dest.IsAcceptingApplications, opt => opt.MapFrom(src =>
                    src.jobStatus == JobStatus.Published && (!src.ApplicationDeadlineUtc.HasValue || src.ApplicationDeadlineUtc > DateTime.UtcNow)))
                .ForMember(dest => dest.IsSavedByCurrentUser, opt => opt.Ignore())
                .ForMember(dest => dest.DistanceKm, opt => opt.Ignore());

            CreateMap<Jobs, JobDetailsResponseDTO>()
                .IncludeBase<Jobs, JobListItemsResponseDTO>()
                .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationCount, opt => opt.Ignore());

            CreateMap<JobRequirements, JobRequirementResponse>()
                .ForMember(dest => dest.SkillName, opt => opt.MapFrom(src => src.Skill != null ? src.Skill.Name : src.Name))
                .ForMember(dest => dest.MinYearsExperience, opt => opt.MapFrom(src => ParseYears(src.ExpectedValue)));

            CreateMap<Domain.Entities.Jobs, JobResponseDTO>()
                .ForMember(dest => dest.PostedAt, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<JobRequestDTO, Domain.Entities.Jobs>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByIp, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedByIp, opt => opt.Ignore())
                .ForMember(dest => dest.LocationPoint, opt => opt.Ignore())
                .ForMember(dest => dest.JobRequirements, opt => opt.Ignore())
                .ForMember(dest => dest.HrProfile, opt => opt.Ignore())
                .ForMember(dest => dest.JobCategory, opt => opt.Ignore());
        }

        private static int? ParseYears(string? value) =>
            int.TryParse(value, out var years) ? years : null;
    }
}
