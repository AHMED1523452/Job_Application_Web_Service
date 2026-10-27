
namespace JobApplication.Domain.Entities
{
    /// <summary>
    /// the number of guests that viewed this job now or viewed already 
    ///                 using the header and that's meaning that should be authenticated or signned in
    /// </summary>
    public  class JobView
    {
        public Guid Id { get; set; }

        // FK -> Job (OnDelete: Cascade)
        public Guid JobId { get; set; }
        public Jobs Job { get; set; } = null!;

        // FK -> ApplicationUser (OnDelete: SetNull, nullable for anonymous views)
        public Guid? UserId { get; set; }

        public DateTime ViewedAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
