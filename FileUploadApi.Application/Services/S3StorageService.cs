using Amazon.S3;
using Amazon.S3.Transfer;
using FileUploadApi.Domain.Options;
using Microsoft.Extensions.Options;

namespace FileUploadApi.Application.Services
{
    public class S3StorageService: IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly StorageOptions _options;
        
        public S3StorageService(IAmazonS3 s3Client, IOptions<StorageOptions> options) 
        {
            _s3Client = s3Client;
            _options = options.Value;
        }
        
        public async Task<string> UploadPhotoAsync(Stream fileStream, string fileName, string contentType)
        {
            return await UploadFileAsync(fileStream, fileName, contentType, _options.BucketNamePhotos);
        }
        
        public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string contentType)
        {
            return await UploadFileAsync(fileStream, fileName, contentType, _options.BucketNameDocuments);
        }
        
        private async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string bucketName)
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