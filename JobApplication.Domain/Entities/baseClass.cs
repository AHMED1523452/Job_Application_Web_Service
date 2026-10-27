namespace JobApplication.Domain.Entities
{
    //. you consider that the cancellation will like Patching or updating
    public class baseClass
    {
        public bool IsActive { get; set; } = true;
        public bool IsArchived { get; set; } = false; 
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; } = null;
        public string CreatedBy { get; set; } = "System";
        public string UpdatedBy { get; set; } = "System";
        public string? DeletedBy { get; set; } = null;
        public string? CreatedByIp { get; set; } = null; 
        public string? UpdatedByIp { get; set; } = null; 
        public string? DeletedByIp { get; set; } = null; 
    }
}