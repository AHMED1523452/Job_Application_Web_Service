using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.Skills.DTOs
{
    public sealed record CreateSkillsRequestDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public long SkillCategoryId { get; set; }
    }
}
