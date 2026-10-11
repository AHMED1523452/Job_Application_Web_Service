using AutoMapper;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Mapping
{
    public class CandidateProfile : Profile
    {
        public CandidateProfile()
        {
            CreateMap<Domain.Entities.CandidateProfile, CandidateResponseDTO>()
                .ForMember(dest => dest.Experiences, opt => opt.MapFrom(src => src.experiences))
                .ForMember(dest => dest.Educations, opt => opt.MapFrom(src => src.educations))
                .ForMember(dest => dest.Certifications, opt => opt.MapFrom(src => src.certifications))
                .ForMember(dest => dest.preference, opt => opt.MapFrom(src => src.Preference));

            CreateMap<CandidateResponseDTO, Domain.Entities.CandidateProfile>()
                .ForMember(dest => dest.experiences, opt => opt.MapFrom(src => src.Experiences))
                .ForMember(dest => dest.educations, opt => opt.MapFrom(src => src.Educations))
                .ForMember(dest => dest.certifications, opt => opt.MapFrom(src => src.Certifications))
                .ForMember(dest => dest.Preference, opt => opt.MapFrom(src => src.preference))
                .ForMember(dest => dest.ProfileImage, opt => opt.Ignore())
                .ForMember(dest => dest.Applications, opt => opt.Ignore())
                .ForMember(dest => dest.JobRecommendations, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateRecommendations, opt => opt.Ignore())
                .ForMember(dest => dest.savedJobs, opt => opt.Ignore());

            CreateMap<CandidateRequestDTO, Domain.Entities.CandidateProfile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByIp, opt => opt.Ignore())

                // Child collections are initialized explicitly on create and synchronized
                // against tracked entities on update. Mapping them here would replace them.
                .ForMember(dest => dest.Skills, opt => opt.Ignore())
                .ForMember(dest => dest.experiences, opt => opt.Ignore())
                .ForMember(dest => dest.educations, opt => opt.Ignore())
                .ForMember(dest => dest.certifications, opt => opt.Ignore())
                .ForMember(dest => dest.Preference, opt => opt.Ignore())
                .ForMember(dest => dest.CVs, opt => opt.Ignore())
                .ForMember(dest => dest.Applications, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImage, opt => opt.Ignore());

            CreateMap<CandidateSkillRequestDTO, CandidateSkill>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore())
                .ForMember(dest => dest.Skill, opt => opt.Ignore());
            CreateMap<CandidateExperienceRequestDTO, CandidateExperience>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore());
            CreateMap<CandidateEducationRequestDTO, CandidateEducation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore());
            CreateMap<CandidateCertificationRequestDTO, CandidateCertification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore());
            CreateMap<CandidatePreferenceRequestDTO, CandidatePreference>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore());

            CreateMap<CandidateSkill, CandidateSkillResponseDTO>();
            CreateMap<CandidateExperience, CandidateExperienceResponseDTO>();
            CreateMap<CandidateEducation, CandidateEducationResponseDTO>();
            CreateMap<CandidateCertification, CandidateCertificationResponseDTO>();
            CreateMap<CandidateCVs, CandidateCvResponseDTO>();
            CreateMap<UploadCandidateCvRequestDTO, CandidateCVs>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore())
                .ForMember(dest => dest.OriginalFileName, opt => opt.MapFrom(src => Path.GetFileName(src.FileName)))
                .ForMember(dest => dest.StoragePath, opt => opt.Ignore())
                .ForMember(dest => dest.ExtractedText, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessingStatus, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedAtUtc, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessedAtUtc, opt => opt.Ignore())
                .ForMember(dest => dest.FileHash, opt => opt.Ignore())
                .ForMember(dest => dest.Applications, opt => opt.Ignore());
            CreateMap<CandidatePreference, CandidatePreferenceResponseDTO>();

            CreateMap<CandidateSkillResponseDTO, CandidateSkill>()
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore())
                .ForMember(dest => dest.Skill, opt => opt.Ignore());

            CreateMap<CandidateExperienceResponseDTO, CandidateExperience>()
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore());

            CreateMap<CandidateEducationResponseDTO, CandidateEducation>()
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore());

            CreateMap<CandidateCertificationResponseDTO, CandidateCertification>()
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore());

            CreateMap<CandidateCvResponseDTO, CandidateCVs>()
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore())
                .ForMember(dest => dest.Applications, opt => opt.Ignore());

            CreateMap<CandidatePreferenceResponseDTO, CandidatePreference>()
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore());

        }
    }
}
