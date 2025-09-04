using FileUploadApi.Domain.Entities;
using FileUploadApi.Domain.Repositories;
using FileUploadApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FileUploadApi.Infrastructure.Repositories
{
    public class UserPhotoRepository: IUserPhotoRepository
    {
        private readonly AppDbContext _dbContext;
        
        public UserPhotoRepository(AppDbContext context)
        {
            _dbContext = context;
        }
        
        public async Task<UserPhoto?> GetByIdAsync(Guid id)
        {
            return await _dbContext.UserPhotos.FirstOrDefaultAsync(up => up.UserId == id);
        }

        public async Task AddAsync(UserPhoto photo)
        {
            await _dbContext.UserPhotos.AddAsync(photo);
        }

        public Task UpdateAsync(UserPhoto photo)
        {
            _dbContext.UserPhotos.Update(photo);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

