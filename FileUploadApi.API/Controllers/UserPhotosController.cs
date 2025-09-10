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
        private readonly IUserPhotoService _userPhotoService;
        private readonly ILogger<UserPhotosController> _logger;

        public UserPhotosController(IUserPhotoService userPhotoService, ILogger<UserPhotosController> logger)
        {

            _userPhotoService = userPhotoService;
            _logger = logger;
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> UploadPhoto(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }
            
            var newFileUrl = await _userPhotoService.UploadPhotoAsync(userId, file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
            if (string.IsNullOrEmpty(newFileUrl))
                return BadRequest("Failed to upload file");

            return Ok(new { PhotoUrl = newFileUrl });
        }
        
        // delete a photo
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeletePhoto(Guid userId, [FromQuery] string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                return BadRequest("No file path provided");
            }
            
            var isdeleted = await _userPhotoService.DeletePhotoAsync(userId, filepath);
            if (!isdeleted)
                return BadRequest("Failed to delete file");
            
            return Ok();
            
        }
    }
}