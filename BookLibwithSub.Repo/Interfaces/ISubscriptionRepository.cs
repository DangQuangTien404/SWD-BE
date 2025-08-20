using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Repo.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<Subscription?> GetByIdWithPlanAsync(int id);
        Task<Subscription?> GetActiveByUserAsync(int userId);
        Task<Subscription?> GetLatestByUserAsync(int userId);
        Task AddAsync(Subscription subscription);
        Task UpdateAsync(Subscription subscription);
    }
}
