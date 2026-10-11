using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Interfaces
{
    public interface IFileStorageService 
    {
        Task<string> UploadAsync(Stream fileStream,
                                string fileName,
                                string bucketName,
                                string contentType,
                                string folder,
                                CancellationToken cancellationToken = default);
        
        Task<Stream> DownloadAsync(string storageKey,
                                  string bucketName,
                                  CancellationToken cancellationToken = default);
        
        Task DeleteAsync(string storageKey,
                        string bucketName,
                        CancellationToken cancellationToken = default);
        
        Task<string> GetSignedURLAsync(string storageKey,
                                            string bucketName,
                                            int expireDate,
                                            CancellationToken cancellationToken = default);
    }
}
