using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using JobApplication.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace JobApplication.Infrastructure.Services.Storage
{
    public class SupabaseStorageService : IFileStorageService
    {
        private readonly AmazonS3Client s3Client;
        public SupabaseStorageService(IOptions<SupabaseStorageServiceOptions> options)
        {
             var puttingExistingCred = new BasicAWSCredentials(
                 options.Value.AccessKey,
                 options.Value.SecretKey);

            s3Client= new AmazonS3Client(puttingExistingCred, new AmazonS3Config
            {
                ServiceURL = options.Value.Endpoint,
                AuthenticationRegion = options.Value.Region,
                ForcePathStyle = true
            });
        }

        public async Task<string> UploadAsync(Stream fileStream,
                                              string fileName,
                                              string bucketName,
                                              string contentType,
                                              string folder,
                                              CancellationToken cancellationToken = default)
        {
            var safeFileName = Path.GetFileName(fileName);

            var storageKey = $"{folder}/{Guid.NewGuid()}_{safeFileName}";

            //. request to the supabase server for another uploading 
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = storageKey,
                ContentType = contentType,
                InputStream = fileStream
            };
            await s3Client.PutObjectAsync(request, cancellationToken);

            //. The URL that will uploaded in db
            return storageKey;
        }

        //. downloading in ram for processes 
        public async Task<Stream> DownloadAsync(string storageKey,
                                                string bucketName,
                                                CancellationToken cancellationToken = default)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = storageKey
            };


            //. get the file from the supabase server 
            var response = await s3Client.GetObjectAsync
                (request, cancellationToken
                );
            //. For reading 
            var memoryStream = new MemoryStream();

            //. Copying the file into the ram
            await response.ResponseStream.CopyToAsync(memoryStream,
                cancellationToken);

            memoryStream.Position = 0;

            return memoryStream; //. if there another processes 
        }

        public async Task DeleteAsync(string storageKey,
                                      string bucketName,
                                      CancellationToken cancellationToken = default)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = storageKey
            };

            await s3Client.DeleteObjectAsync(request, cancellationToken);
        }

        public Task<string> GetSignedURLAsync(string storageKey,
                                            string bucketName,
                                            int expireDate,
                                            CancellationToken cancellationToken = default)
        {
            //. create new url for the file that setted as private and the user needs to open this file
            var request = new GetPreSignedUrlRequest
            {
                Key = storageKey,
                BucketName = bucketName,
                Expires = DateTime.UtcNow.AddMinutes(expireDate)
            };

            return Task.FromResult(s3Client.GetPreSignedURL(request));
        }

    }
}
