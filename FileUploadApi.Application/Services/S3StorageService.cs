using Amazon.S3;
using Amazon.S3.Transfer;
using FileUploadApi.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileUploadApi.Application.Services
{
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly StorageOptions _options;
        private readonly ILogger<S3StorageService> _logger;

        public S3StorageService(IAmazonS3 s3Client, IOptions<StorageOptions> options, ILogger<S3StorageService> logger)
        {
            _s3Client = s3Client;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> UploadPhotoAsync(Stream fileStream, string fileName, string contentType)
        {
            return await UploadFileAsync(fileStream, fileName, contentType, _options.BucketNamePhotos);
        }

        public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string contentType)
        {
            return await UploadFileAsync(fileStream, fileName, contentType, _options.BucketNameDocuments);
        }

        public async Task<bool> DeletePhotoAsync(string fileName)
        {
            if (ObjectExistAsync(_options.BucketNamePhotos, fileName).Result)
            {
                return await DeleteFileAsync(fileName, _options.BucketNamePhotos);
            }
            
            return false;
        }

        private async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType,
            string bucketName)
        {
            // Ensure bucket exists before uploading
            await EnsureBucketExistsAsync(bucketName);

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

        private async Task<bool> DeleteFileAsync(string fileName, string bucketName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                var deleteObjectRequest = new Amazon.S3.Model.DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName
                };
                await _s3Client.DeleteObjectAsync(deleteObjectRequest);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting file from S3");
                return false;
            }
        }


        // exists object in bucket
        private async Task<bool> ObjectExistAsync(string bucketName, string objectKey)
        {
            try
            {
                var response = await _s3Client.GetObjectMetadataAsync(bucketName, objectKey);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (AmazonS3Exception e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("File not found in S3 bucket: {objectKey}", objectKey);
                    return false;
                }

                throw;
            }
        }

        // Ensure bucket exists, create if it doesn't
        private async Task EnsureBucketExistsAsync(string bucketName)
        {
            try
            {
                var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
                
                if (!bucketExists)
                {
                    _logger.LogInformation("Creating S3 bucket: {BucketName}", bucketName);
                    
                    var putBucketRequest = new Amazon.S3.Model.PutBucketRequest
                    {
                        BucketName = bucketName,
                        UseClientRegion = true
                    };
                    
                    await _s3Client.PutBucketAsync(putBucketRequest);
                    _logger.LogInformation("Successfully created S3 bucket: {BucketName}", bucketName);
                }
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError(e, "Error ensuring bucket exists: {BucketName}", bucketName);
                throw;
            }
        }
    }
}