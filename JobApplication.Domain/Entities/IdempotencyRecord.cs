
namespace JobApplication.Domain.Entities
{
    public class IdempotencyRecord
    {
        public string Key { get; set; } = default!;                 
        public string RequestHash { get; set; } = default!;          
        public string Status { get; set; } = "Processing";           
        public int? ResponseCode { get; set; }                       
        public string? ResponseBody { get; set; }                    
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
