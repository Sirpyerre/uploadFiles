using FileUploadApi.Application.helpers;
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

            var filePath = FilePathHelper.GetPhotoPath(userId, fileName);
            var newFileUrl = await _storageService.UploadPhotoAsync(fileStream, filePath, contentType);

            if (existingPhoto != null)
            {
                try
                {
                    if (!await _storageService.DeletePhotoAsync(existingPhoto.FileName))
                    {
                        _logger.LogWarning("Failed to delete existing photo from S3. Aborting update.");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to delete existing photo from S3. Aborting update.");
                    return string.Empty;
                }

                existingPhoto.FileName = filePath;
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
                        FileName = filePath,
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

        public async Task<bool> DeletePhotoAsync(Guid userId, string fileName)
        {
            var existingPhoto = _photoRepository.GetByIdAsync(userId).Result;
            if (existingPhoto != null && existingPhoto.FileName != fileName)
            {
                _logger.LogWarning("Photo not found for user {UserId} with file name {FileName}", userId, fileName);
                return false;
            }

            try
            {
                if (!await _storageService.DeletePhotoAsync(existingPhoto.FileName))
                {
                    _logger.LogWarning("Failed to delete existing photo from S3. Aborting update.");
                    return false;
                }

                // delete row from database
                if (!await _photoRepository.DeleteAsync(userId))
                {
                    _logger.LogWarning("Failed to delete existing photo from database. Aborting update.");
                    return false;
                }

                await _photoRepository.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete existing photo from S3. Aborting update.");
                return await Task.FromResult(false);
            }


            return true;
        }
    }
}