using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookLibwithSub.Repo.repository
{
    public class SubscriptionPlanRepository : ISubscriptionPlanRepository
    {
        private readonly AppDbContext _context;
        public SubscriptionPlanRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubscriptionPlan>> GetAllAsync()
        {
            return await _context.SubscriptionPlans
                .AsNoTracking()
                .OrderBy(p => p.Price)
                .ThenBy(p => p.PlanName)
                .ToListAsync();
        }

        public async Task<SubscriptionPlan?> GetByIdAsync(int id)
        {
            return await _context.SubscriptionPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.SubscriptionPlanID == id);
        }

        public async Task AddAsync(SubscriptionPlan plan)
        {

            if (await ExistsByNameAsync(plan.PlanName))
                throw new InvalidOperationException($"Plan '{plan.PlanName}' already exists.");

            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SubscriptionPlan plan)
        {

            if (await ExistsByNameAsync(plan.PlanName, plan.SubscriptionPlanID))
                throw new InvalidOperationException($"Plan '{plan.PlanName}' already exists.");

            _context.SubscriptionPlans.Update(plan);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.SubscriptionPlanID == id);

            if (plan != null)
            {
                _context.SubscriptionPlans.Remove(plan);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByNameAsync(string planName, int? exceptId = null)
        {
            var normalized = planName.Trim().ToLower();
            var q = _context.SubscriptionPlans.AsNoTracking()
                .Where(p => p.PlanName.ToLower() == normalized);

            if (exceptId.HasValue)
                q = q.Where(p => p.SubscriptionPlanID != exceptId.Value);

            return await q.AnyAsync();
        }
    }
}
