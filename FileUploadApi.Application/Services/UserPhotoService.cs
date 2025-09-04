using Microsoft.Extensions.Logging;
using FileUploadApi.Domain.Entities;
using FileUploadApi.Domain.Options;
using FileUploadApi.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace FileUploadApi.Application.Services
{
    public class UserPhotoService : IUserPhotoService
    {
        private readonly ILogger<UserPhotoService> _logger;
        private readonly IStorageService _storageService;
        private readonly IUserPhotoRepository _photoRepository;

        public UserPhotoService(
            IUserPhotoRepository photoRepository,
            IStorageService storageService,
            ILogger<UserPhotoService> logger
        )
        {
            _logger = logger;
            _photoRepository = photoRepository;
            _storageService = storageService;
        }

        public async Task<string> UploadPhotoAsync(Guid userId, Stream fileStream, string fileName, string contentType,
            long fileSize)
        {
            var existingPhoto = _photoRepository.GetByIdAsync(userId).Result;

            var newFileName = $"{Guid.NewGuid()}_{fileName}";
            var newFileUrl = await _storageService.UploadPhotoAsync(fileStream, fileName, contentType);

            if (existingPhoto != null)
            {
                try
                {
                    if (!await _storageService.DeletePhotoAsync(existingPhoto.FileName))
                    {
                        _logger.LogWarning("Failed to delete existing photo from S3. Aborting update.");
                        return string.Empty;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to delete existing photo from S3. Aborting update.");
                    return string.Empty;
                }

                existingPhoto.FileName = newFileName;
                existingPhoto.FileUrl = newFileUrl;
                existingPhoto.FileSize = fileSize;
                existingPhoto.ContentType = contentType;
                existingPhoto.UploadedAt = DateTime.UtcNow;

                await _photoRepository.UpdateAsync(existingPhoto);
            }
            else
            {
                try
                {
                    var userPhoto = new UserPhoto
                    {
                        UserId = userId,
                        FileName = newFileName,
                        FileUrl = newFileUrl,
                        ContentType = contentType,
                        FileSize = fileSize,
                        UploadedAt = DateTime.UtcNow
                    };

                    await _photoRepository.AddAsync(userPhoto);
                }
                catch (Exception var)
                {
                    _logger.LogError(var, "Failed to upload photo to S3. Aborting update.");
                    return string.Empty;
                }
            }

            await _photoRepository.SaveChangesAsync();
            
            return newFileUrl;
        }
    }
}