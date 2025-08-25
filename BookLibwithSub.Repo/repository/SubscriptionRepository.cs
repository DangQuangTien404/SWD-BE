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
        public SubscriptionRepository(AppDbContext context) => _context = context;

        public Task<Subscription?> GetByIdWithPlanAsync(int id) =>
            _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(s => s.SubscriptionID == id);

        public async Task<Subscription?> GetActiveByUserAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await _context.Subscriptions
                .Where(s => s.UserID == userId && s.Status == "Active" && s.StartDate <= now && s.EndDate > now)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }

        public Task<Subscription?> GetLatestByUserAsync(int userId) =>
            _context.Subscriptions
                .Where(s => s.UserID == userId)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

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
        public async Task ActivateAsync(int subscriptionId, DateTime startUtc, DateTime endUtc)
        {
            if (startUtc.Kind != DateTimeKind.Utc)
                startUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
            if (endUtc.Kind != DateTimeKind.Utc)
                endUtc = DateTime.SpecifyKind(endUtc, DateTimeKind.Utc);

            var sub = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.SubscriptionID == subscriptionId);

            if (sub == null)
                throw new InvalidOperationException("Subscription not found");

            sub.Status = "Active";
            sub.StartDate = startUtc;
            sub.EndDate = endUtc;

            await _context.SaveChangesAsync();
        }
    }
}
