using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Service.Interfaces
{
    public interface ISubscriptionPlanService
    {
        Task<IEnumerable<SubscriptionPlan>> GetAllAsync();
        Task<SubscriptionPlan> AddAsync(SubscriptionPlan plan);
        Task<SubscriptionPlan?> GetByIdAsync(int id);
        Task UpdateAsync(int id, SubscriptionPlan plan);
        Task DeleteAsync(int id);
    }
}
