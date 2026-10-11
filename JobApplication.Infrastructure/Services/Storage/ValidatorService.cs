using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;

namespace JobApplication.Infrastructure.Services.Storage
{
    public  class ValidatorService : IValidatorService
    {
        private static readonly Dictionary<FileCategory, string[]> allowedExtensions = new()
        {
                        [FileCategory.CandidateCV] =
                            [".pdf", ".docx", ".doc"],

                        [FileCategory.ProfileImage] =
                            [".jpg", ".jpeg", ".png", ".webp"],

                        [FileCategory.CompanyLogo] =
                            [".jpg", ".jpeg", ".png", ".webp"],

                        [FileCategory.JobImage] =
                        [".jpg", ".jpeg", ".png", ".webp"]
        };

        private static readonly Dictionary<FileCategory, long> allowedFileSizes = new()
        {
            [FileCategory.CandidateCV] = 5  * 1024 * 1024, // 5 MB
            [FileCategory.ProfileImage] = 2 * 1024 * 1024, // 2 MB
            [FileCategory.CompanyLogo] = 5 * 1024 * 1024, // 5 MB
            [FileCategory.JobImage] = 10 * 1024 * 1024 // 10 MB
        };

        private static readonly Dictionary<FileCategory, string[]> allowedContentTypes = new()
        {
            [FileCategory.CandidateCV] =
                ["application/pdf", "application/msword",
                                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
            [FileCategory.ProfileImage] =
                ["image/jpeg", "image/png", "image/webp"],
            [FileCategory.CompanyLogo] =
                ["image/jpeg", "image/png", "image/webp"],
            [FileCategory.JobImage] =
                ["image/jpeg", "image/png", "image/webp"]
        };

        public bool IsValidFileExtension(
                        string fileName,
                        string contentType,
                        long FileSize,
                        FileCategory fileCategory)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!allowedFileSizes.TryGetValue(fileCategory, out var maxSize))
                return false;

            if(!allowedContentTypes.TryGetValue(fileCategory, out var contentTypes))
                return false;

            if (!allowedExtensions.TryGetValue(fileCategory, out var extensions))
                return false;

            if (FileSize > maxSize)
                return false;

            if (!contentTypes.Contains(contentType))    
                return false;   

            if (!extensions.Contains(extension))
                return false;

            return true;
        }
    }
}
