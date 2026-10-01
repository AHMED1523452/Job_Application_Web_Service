using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities{
    public class JobCategory : baseClass{
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Jobs> Jobs { get; set; } = new List<Jobs>();
    }
}