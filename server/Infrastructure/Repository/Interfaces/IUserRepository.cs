using server.Domain.Entities;

namespace server.Infrastructure.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User> AddAsync(User user);
        Task<User?> GetByIdAsync(Guid id);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);
    }
}