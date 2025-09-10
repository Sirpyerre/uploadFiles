namespace FileUploadApi.Application.Services;

public interface IUserPhotoService
{
    Task<string> UploadPhotoAsync(Guid userId, Stream fileStream, string fileName, string contentType, long fileSize);
    Task<bool> DeletePhotoAsync(Guid userId, string fileName);
}