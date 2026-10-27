using System.Threading;
using System.Threading.Tasks;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Interfaces
{
    /// <summary>
    /// Service for extracting text from documents and images stored in Supabase storage.
    /// Supports PDF, DOCX, and image files (OCR) based on the <see cref="FileCategory"/>.
    /// </summary>
    public interface IExtractorService
    {
        /// <summary>
        /// Downloads a file from Supabase storage and extracts its text content.
        /// </summary>
        /// <param name="storageKey">The storage key (path) of the file in Supabase.</param>
        /// <param name="bucketName">The Supabase bucket name.</param>
        /// <param name="fileCategory">The category of the file (determines extraction strategy).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The extracted text content.</returns>
        Task<string> ExtractTextAsync(
            string storageKey,
            string bucketName,
            FileCategory fileCategory,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts text from a byte array (already downloaded from storage).
        /// </summary>
        /// <param name="fileBytes">The file content as bytes.</param>
        /// <param name="fileExtension">The file extension (e.g., .pdf, .docx, .png).</param>
        /// <param name="fileCategory">The category of the file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The extracted text content.</returns>
        Task<string> ExtractTextFromBytesAsync(
            byte[] fileBytes,
            string fileExtension,
            FileCategory fileCategory,
            CancellationToken cancellationToken = default);
    }
}
