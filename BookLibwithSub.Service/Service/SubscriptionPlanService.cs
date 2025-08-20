using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using BookLibwithSub.Service.Interfaces;

namespace BookLibwithSub.Service.Service
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly ISubscriptionPlanRepository _repo;
        public SubscriptionPlanService(ISubscriptionPlanRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<SubscriptionPlan> AddAsync(SubscriptionPlan plan)
        {
            await _repo.AddAsync(plan);
            return plan;
        }

        public Task<SubscriptionPlan?> GetByIdAsync(int id)
        {
            return _repo.GetByIdAsync(id);
        }

        public async Task UpdateAsync(int id, SubscriptionPlan plan)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Subscription plan not found");

            existing.PlanName = plan.PlanName;
            existing.DurationDays = plan.DurationDays;
            existing.MaxPerDay = plan.MaxPerDay;
            existing.MaxPerMonth = plan.MaxPerMonth;
            existing.Price = plan.Price;

            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
