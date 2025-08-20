using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Repo.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int userId);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByTokenAsync(string token);

        Task<User?> GetTrackedByIdAsync(int userId);

        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int userId);

        Task UpdateTokenAsync(int userId, string? token);
    }
}
