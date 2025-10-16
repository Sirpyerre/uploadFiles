using FileUploadApi.API.Models;
using FluentValidation;


public class FileUploadDtoValidator : AbstractValidator<UploadPhotoRequest>
{
    public FileUploadDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Missing userId")
            .MinimumLength(10).WithMessage("UserId must be at least 10 characters long");

        RuleFor(x => x.File)
            .NotNull().WithMessage("Missing file")
            .Must(BeValidImageFile).WithMessage("No allowed extension")
            .Must(HaveValidSize).WithMessage("File size must be less than 2MB");
    }

    private bool BeValidImageFile(IFormFile? file)
    {
        if (file == null) return true;
        var allowedContentTypes = new[] { "image/png", "image/jpg", "image/jpeg" };
        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };

        var contentTypeValid = allowedContentTypes.Contains(file.ContentType?.ToLower());
        var extensionValid = allowedExtensions.Contains(Path.GetExtension(file.FileName)?.ToLower());

        return contentTypeValid && extensionValid;
    }
    
    private bool HaveValidSize(IFormFile? file)
    {
        if (file == null) return true;

        const long maxSizeInBytes = 2 * 1024 * 1024;
        return file.Length > 0 && file.Length <= maxSizeInBytes;
    }
}