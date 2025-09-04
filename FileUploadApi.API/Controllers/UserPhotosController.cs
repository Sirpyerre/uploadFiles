using FileUploadApi.Application.Services;
using FileUploadApi.Domain.Entities;
using FileUploadApi.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileUploadApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserPhotosController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<UserPhotosController> _logger;

        public UserPhotosController(IStorageService storageService, AppDbContext context, IConfiguration config,
            ILogger<UserPhotosController> logger)
        {
            _storageService = storageService;
            _dbContext = context;
            _logger = logger;
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> UploadPhoto(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var exisingPhoto = await _dbContext.UserPhotos
                .FirstOrDefaultAsync(up => up.UserId == userId);

            var newFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var newFileUrl = string.Empty;


            if (exisingPhoto != null)
            {
                try
                {
                    if (!await _storageService.DeletePhotoAsync(exisingPhoto.FileName))
                    {
                        _logger.LogWarning("Failed to delete existing photo from S3. Aborting update.");
                        return StatusCode(500, "Failed to update photo due to a storage service error.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete existing photo from S3. Aborting update.");
                    return StatusCode(500, "Failed to update photo due to a storage service error.");
                }

                await using var fileStream = file.OpenReadStream();
                newFileUrl = await _storageService.UploadPhotoAsync(fileStream, newFileName, file.ContentType);

                exisingPhoto.FileName = newFileName;
                exisingPhoto.FileUrl = newFileUrl;
                exisingPhoto.ContentType = file.ContentType;
                exisingPhoto.FileSize = file.Length;
                exisingPhoto.UploadedAt = DateTime.UtcNow;

                _dbContext.UserPhotos.Update(exisingPhoto);
            }
            else
            {
                try
                {
                    await using var fileStream = file.OpenReadStream();
                    newFileUrl = await _storageService.UploadPhotoAsync(fileStream, newFileName, file.ContentType);

                    var userPhoto = new UserPhoto
                    {
                        Id = Guid.NewGuid(),
                        FileName = newFileName,
                        FileUrl = newFileUrl,
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        UserId = userId,
                        UploadedAt = DateTime.UtcNow
                    };

                    _dbContext.UserPhotos.Add(userPhoto);
                } catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload photo to S3. Aborting update.");
                    return StatusCode(500, "Failed to update photo due to a storage service error.");
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { PhotoUrl = newFileUrl });
        }
    }
}