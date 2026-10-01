using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class SearchHistory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // FK -> ApplicationUser (OnDelete: SetNull, nullable for anonymous search)
        public Guid? UserId { get; set; }

        public string SearchQuery { get; set; } = null!;
        public string? FiltersJson { get; set; }
        public int ResultsCount { get; set; }
        public DateTime SearchedAt { get; set; }
    }
}
