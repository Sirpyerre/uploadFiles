using FileUploadApi.API.Models;
using FileUploadApi.Application.Services;
using FileUploadApi.Domain.Entities;
using FileUploadApi.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FileUploadApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserPhotosController : ControllerBase
    {
        private readonly IUserPhotoService _userPhotoService;
        private readonly ILogger<UserPhotosController> _logger;
        IValidator<UploadPhotoRequest> _uploadValidator;

        public UserPhotosController(
            IUserPhotoService userPhotoService,
            ILogger<UserPhotosController> logger,
            IValidator<UploadPhotoRequest> uploadValidator
            )
        {

            _userPhotoService = userPhotoService;
            _logger = logger;
            _uploadValidator = uploadValidator;
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> UploadPhoto(Guid userId, IFormFile file)
        {

            var request = new UploadPhotoRequest
            {
                UserId = userId.ToString(),
                File = file
            };

            var validationResult = await _uploadValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));

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