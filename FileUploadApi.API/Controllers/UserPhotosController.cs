using FileUploadApi.Application.Services;
using FileUploadApi.Domain.Entities;
using FileUploadApi.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace FileUploadApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserPhotosController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly AppDbContext _dbContext;
        private readonly string _bucketName;

        public UserPhotosController(IStorageService storageService, AppDbContext context, IConfiguration config)
        {
            _storageService = storageService;
            _dbContext = context;
            _bucketName = config["Storage:BucketNamePhotos"];
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

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            await using var stream = file.OpenReadStream();
            
            var fileUrl = await _storageService.UploadPhotoAsync(stream, file.FileName, file.ContentType);

            var userPhoto = new UserPhoto
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                FileUrl = fileUrl,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UserId = userId,
                UploadedAt = DateTime.UtcNow
            };

            _dbContext.UserPhotos.Add(userPhoto);
            await _dbContext.SaveChangesAsync();

            return Ok(new { PhotoUrl = fileUrl });
        }
    }
}