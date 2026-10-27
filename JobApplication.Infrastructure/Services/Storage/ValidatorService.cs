using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using System.IO;
using System.Linq;

namespace JobApplication.Infrastructure.Services.Storage
{
    public class ValidatorService : IValidatorService
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
            [FileCategory.CandidateCV] = 5 * 1024 * 1024, // 5 MB
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

            if (!allowedContentTypes.TryGetValue(fileCategory, out var contentTypes))
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

        // NEW VALIDATOR 1: File Signature (Magic Bytes)
        // Reason for addition: Extension alone can be forged (e.g., renaming .jpg to .pdf).
        // This checks the actual file header bytes to verify the real file type.
        // Runs BEFORE uploading to Supabase (fail-fast, save bandwidth/storage).
        public bool IsValidFileSignature(byte[] fileBytes, string fileExtension)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                return false;

            var ext = (fileExtension ?? string.Empty).ToLowerInvariant();

            using var ms = new MemoryStream(fileBytes);
            using var reader = new BinaryReader(ms);

            // Read first 4 bytes to determine the real file type
            var headerBytes = reader.ReadBytes(Math.Min(4, fileBytes.Length));

            return ext switch
            {
                ".pdf" => IsPdfSignature(headerBytes),
                ".docx" => IsDocxSignature(headerBytes),
                ".doc" => IsDocSignature(headerBytes),
                _ => true // for other extensions (images etc.) no strict signature check is enforced here
            };
        }

        // NEW VALIDATOR 2: PDF OCR Need Detection
        // Reason for addition: Some PDF files are scanned images with no extractable text.
        // This must be called AFTER text extraction (not during initial validation),
        // because you cannot know if text is empty until you try to extract it.
        // Runs AFTER downloading from Supabase and AFTER ExtractPDFText/ExtractDOCXText.
        public bool IsPdfNeedsOcr(string? extractedText)
        {
            return string.IsNullOrWhiteSpace(extractedText);
        }

        // NEW VALIDATOR 3: Comprehensive CV File Validator
        // Reason for addition: Combines all pre-upload checks into one method.
        // Does NOT include extractedText because text extraction happens AFTER upload.
        // Runs BEFORE uploading to Supabase.
        public (bool IsValid, string Message) ValidateCandidateCVFile(
            byte[] fileBytes,
            string fileName,
            string contentType,
            long fileSize,
            FileCategory fileCategory,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Basic validation of extension, size, and type (existing logic)
            if (!IsValidFileExtension(fileName, contentType, fileSize, fileCategory))
                return (false, "File type, size, or extension is not allowed.");

            // Step 2: Real file signature validation (new security check)
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!IsValidFileSignature(fileBytes, extension))
                return (false, "Invalid file: content does not match the claimed extension.");

            return (true, "File is valid.");
        }

        // HELPER METHODS: File signature checking (Magic Bytes)
        // Reason for addition: Required by IsValidFileSignature to identify real file types.

        private static bool IsPdfSignature(byte[] header)
        {
            // PDF signature starts with: %PDF (0x25 0x50 0x44 0x46)
            return header.Length >= 4 &&
                   header[0] == 0x25 && // %
                   header[1] == 0x50 && // P
                   header[2] == 0x44 && // D
                   header[3] == 0x46;   // F
        }

        private static bool IsDocxSignature(byte[] header)
        {
            // DOCX signature starts with: PK (0x50 0x4B) because it is a ZIP-compressed file
            return header.Length >= 2 &&
                   header[0] == 0x50 && // P
                   header[1] == 0x4B;   // K
        }

        private static bool IsDocSignature(byte[] header)
        {
            // Old DOC signature starts with: D0 CF 11 E0
            return header.Length >= 4 &&
                   header[0] == 0xD0 &&
                   header[1] == 0xCF &&
                   header[2] == 0x11 &&
                   header[3] == 0xE0;
        }
    }
}
