
namespace JobApplication.Application.Features.Skills.DTOs
{
    public sealed class SkillsDTO
    {
        public string Name { get; set; } = null!;

        public long? SkillCategoryId { get; set; }

        public string SkillCategoryName { get; set; } = null!;
    }
}
