using System.Security.Cryptography;
using System.Text;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Services;
using JobApplication.Domain.Entities;

namespace JobApplication.Infrastructure.Services
{
    /// <summary>
    /// Implementation of <see cref="IFileHashService"/> using SHA256 algorithm.
    /// Provides file bytes hashing and extracted text duplicate verification with normalization.
    /// </summary>
    public class FileHashService : IFileHashService
    {
        private readonly IUnitOfWork unitOfWork;

        public FileHashService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }


        public string ComputeHash(byte[] fileBytes)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(fileBytes);
            return Convert.ToHexString(hash);
        }

        public async Task<string> ComputeExtractedTextHashAsync(string extractedText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(extractedText))
                return string.Empty;

            // Use NormalizeContentTextService from Application layer for normalization
            var normalized = NormalizeContentTextService.normalizeText(extractedText);

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes( normalized);
            var hash = sha.ComputeHash(bytes); //. Converting the bytes to hash of type (sha 256)
            //. converting the hashing to string data type to be compared with the file hash that existing in db
            return Convert.ToHexString(hash);  //. another converting to a string data type
        } 

        public async Task<bool> IsExtractedTextDuplicateAsync(string extractedTextHash, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(extractedTextHash))
                return false;

            return await unitOfWork.Repository<CandidateCVs>().AnyAsync(predicate: 
                 c => c.FileHash == extractedTextHash, cancellationToken);
        }


        //. Making sure that the file content is not duplicated and refuse if duplicated
        public async Task<bool> IsFileDuplicateAsync(string fileHash, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileHash))
                return false;

            return await unitOfWork.Repository<CandidateCVs>().AnyAsync(predicate:
                 c => c.FileHash == fileHash, cancellationToken);
        }
    }
}
