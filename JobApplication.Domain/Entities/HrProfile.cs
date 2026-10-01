using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities{
    public class HrProfile : baseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        // HR Information
        public string Name { get; set; } = null!;
        public string? Address { get; set; }

        // Company Information
        public string CompanyName { get; set; } = null!;
        public string? CompanyDescription { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyLogo { get; set; }
        public string? CompanyIndustry { get; set; }
        public string? CompanySize { get; set; }
        public string? CompanyLocation { get; set; }
        public string? CompanyType { get; set; }

        public ICollection<Jobs> Jobs { get; set; } = new List<Jobs>();
    }
}