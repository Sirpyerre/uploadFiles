namespace FileUploadApi.Application.Services
{
    public interface IStorageService
    {
        Task<string> UploadPhotoAsync(Stream fileStream, string fileName, string contentType);
        Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string contentType);
    }
}