using Amazon.S3;
using Amazon.S3.Transfer;

namespace FileUploadApi.Application.Services
{
    public class S3StorageService: IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        
        public S3StorageService(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }
        
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string bucketName)
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = fileName,
                BucketName = bucketName,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);
            
            return $"https://{bucketName}.s3.localhost.localstack.cloud:4566/{fileName}";
        }
    }
}