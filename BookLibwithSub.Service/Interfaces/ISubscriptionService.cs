using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Service.Models;
using System.Threading.Tasks;

namespace BookLibwithSub.Service.Interfaces
{
    public interface ISubscriptionService
    {
        Task<Transaction> PurchaseAsync(int userId, int planId);
        Task<Transaction> RenewAsync(int userId);
        Task<SubscriptionStatusDto> GetMyStatusAsync(int userId);

    }
}
