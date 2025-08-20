using System;
using System.Linq;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookLibwithSub.Repo.repository
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly AppDbContext _context;
        public SubscriptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Subscription?> GetByIdWithPlanAsync(int id)
        {
            return await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(s => s.SubscriptionID == id);
        }

        public async Task<Subscription?> GetActiveByUserAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await _context.Subscriptions
                .Where(s => s.UserID == userId && s.Status == "Active" && s.StartDate <= now && s.EndDate > now)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }

        public async Task<Subscription?> GetLatestByUserAsync(int userId)
        {
            return await _context.Subscriptions
                .Where(s => s.UserID == userId)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Subscription subscription)
        {
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Subscription subscription)
        {
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
        }
    }
}
