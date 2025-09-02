namespace FileUploadApi.Application.Services
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string bucketName);
    }
}