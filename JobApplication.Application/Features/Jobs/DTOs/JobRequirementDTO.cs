
namespace JobApplication.Application.Features.Jobs.DTOs
{
    public class JobRequirementDTO
    {
        public Guid? SkillId { get; set; }
        public string? Description { get; set; }
        public bool IsRequired { get; set; } = true;
        public int? MinYearsExperience { get; set; }
    }
}
