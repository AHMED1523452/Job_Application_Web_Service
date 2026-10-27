using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Services;
using JobApplication.Domain.Enums;

namespace JobApplication.Infrastructure.Services
{
    /// <summary>
    /// Implementation of <see cref="IExtractorService"/> that downloads files from Supabase storage
    /// and extracts text based on the <see cref="FileCategory"/> (PDF, DOCX, or Image/OCR).
    /// </summary>
    public class ExtractorService : IExtractorService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IOcrService _ocrService;

        public ExtractorService(
            IFileStorageService fileStorageService,
            IOcrService ocrService)
        {
            _fileStorageService = fileStorageService;
            _ocrService = ocrService;
        }

        /// <summary>
        /// Downloads a file from Supabase storage and extracts its text content based on file category.
        /// </summary>
        public async Task<string> ExtractTextAsync(
            string storageKey,
            string bucketName,
            FileCategory fileCategory,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
                throw new System.ArgumentException("Storage key cannot be null or empty.", nameof(storageKey));

            if (string.IsNullOrWhiteSpace(bucketName))
                throw new System.ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));

            using var fileStream = await _fileStorageService.DownloadAsync(
                storageKey,
                bucketName,
                cancellationToken);

            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms, cancellationToken);
            var fileBytes = ms.ToArray();

            var fileExtension = System.IO.Path.GetExtension(storageKey)?.ToLowerInvariant() ?? string.Empty;

            return await ExtractTextFromBytesAsync(fileBytes, fileExtension, fileCategory, cancellationToken);
        }

        /// <summary>
        /// Extracts text from a byte array based on file extension and category.
        /// </summary>
        public async Task<string> ExtractTextFromBytesAsync(
            byte[] fileBytes,
            string fileExtension,
            FileCategory fileCategory,
            CancellationToken cancellationToken = default)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                throw new System.ArgumentException("File bytes cannot be null or empty.", nameof(fileBytes));

            var ext = (fileExtension ?? string.Empty).ToLowerInvariant();

            // PDF files
            if (ext == ".pdf")
            {
                var text = ExtractPDFText.PdfExtractor(fileBytes);
                return text;
            }

            // DOCX files
            if (ext == ".docx" || ext == ".doc")
            {
                var text = ExtractDOCXText.DocxFileTextExtractor(fileBytes);
                return text;
            }

            // Image files - use OCR via ExtractWithOCR
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".tiff" || ext == ".tif")
            {
                var text = _ocrService.ExtractTextFromImage(fileBytes);
                return text;
            }

            // Unsupported file type
            return string.Empty;
        }
    }
}
