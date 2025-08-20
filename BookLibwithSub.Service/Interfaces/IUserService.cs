using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Service.Models.User;

namespace BookLibwithSub.Service.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int userId);
        Task UpdateProfileAsync(int userId, UpdateMeRequest req);
        Task<UserProfileDto> GetProfileAsync(int userId);
    }
}