
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class SkillCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public ICollection<Skill> Skills { get; set; } = [];
    }
}
