
namespace JobApplication.Application.Features.Jobs.DTOs
{
    public class JobRequirementResponse
    {
        public Guid Id { get; set; }
        public Guid? SkillId { get; set; }
        public string? SkillName { get; set; }
        public string? Description { get; set; }
        public bool IsRequired { get; set; }
        public int? MinYearsExperience { get; set; }
    }
}
