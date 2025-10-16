namespace FileUploadApi.API.Models
{
    public class UploadPhotoRequest
    {
        public String UserId { get; set; } = string.Empty;
        public IFormFile File { get; set; } = null!;
    }
}