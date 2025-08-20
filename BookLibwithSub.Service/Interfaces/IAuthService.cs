using System.Threading.Tasks;
using BookLibwithSub.Service.Models;
using BookLibwithSub.Service.Models.User;

namespace BookLibwithSub.Service.Interfaces
{
    public interface IAuthService
    {

        Task RegisterAsync(RegisterRequest request);
        Task<string?> LoginAsync(LoginRequest request);
        Task LogoutAsync(int userId);

        Task UpdateAccountAsync(int userId, UpdateUserRequest request);
        Task DeleteAccountAsync(int userId);
    }
}
