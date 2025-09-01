namespace FileUploadApi.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<UserPhoto> Photos { get; set; } = new List<UserPhoto>();
    public ICollection<UserDocument> Documents { get; set; } = new List<UserDocument>();
}


