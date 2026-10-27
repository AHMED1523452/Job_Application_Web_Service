using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Interfaces
{
    /// <summary>
    /// Abstraction for computing cryptographic hash of file bytes and extracted text.
    /// </summary>
    public interface IFileHashService
    {
        string ComputeHash(byte[] fileBytes);

        Task<string> ComputeExtractedTextHashAsync(string extractedText, CancellationToken cancellationToken = default);

        Task<bool> IsExtractedTextDuplicateAsync(string extractedTextHash, CancellationToken cancellationToken = default);

        /// <summary>Checks whether a CV with the same file-bytes hash already exists (fast, runs inside the upload request).</summary>
        Task<bool> IsFileDuplicateAsync(string fileHash, CancellationToken cancellationToken = default);
    }
}
