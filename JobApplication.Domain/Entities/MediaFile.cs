using JobApplication.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class MediaFile : baseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid MediaFileID { get; set; }
        public string FileName { get; set; } = null!;

        public string StoragePath { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public string FileExtension { get; set; } = null!;

        public long FileSize { get; set; }

        public string? FileHash { get; set; }

        public MediaType MediaType { get; set; }
    }
}
