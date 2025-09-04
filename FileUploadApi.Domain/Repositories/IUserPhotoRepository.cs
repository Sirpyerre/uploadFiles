using FileUploadApi.Domain.Entities;

namespace FileUploadApi.Domain.Repositories
{
    public interface IUserPhotoRepository
    {
        Task<UserPhoto?> GetByIdAsync(Guid id);
        Task AddAsync(UserPhoto photo);
        Task UpdateAsync(UserPhoto photo);
        Task SaveChangesAsync();
    }
}

