using AutoMapper;
using JobApplication.Application.Features.HrProfile.DTOs;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Mapping
{
    public class HrProfileProfile : Profile
    {
        public HrProfileProfile()
        {
            CreateMap<HrProfile, HrProfileResponseDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
                .ForMember(dest => dest.CompanyDescription, opt => opt.MapFrom(src => src.CompanyDescription))
                .ForMember(dest => dest.CompanyWebsite, opt => opt.MapFrom(src => src.CompanyWebsite))
                .ForMember(dest => dest.CompanyLogoPath, opt => opt.MapFrom(src => src.CompanyLogoPath))
                .ForMember(dest => dest.CompanyIndustry, opt => opt.MapFrom(src => src.CompanyIndustry))
                .ForMember(dest => dest.CompanySize, opt => opt.MapFrom(src => src.CompanySize))
                .ForMember(dest => dest.CompanyLocation, opt => opt.MapFrom(src => src.CompanyLocation))
                .ForMember(dest => dest.CompanyType, opt => opt.MapFrom(src => src.CompanyType));
        }
    }
}