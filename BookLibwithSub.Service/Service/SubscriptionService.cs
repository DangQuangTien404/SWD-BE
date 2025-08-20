using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using BookLibwithSub.Service.Interfaces;
using BookLibwithSub.Service.Models;
using System;
using System.Threading.Tasks;

namespace BookLibwithSub.Service.Service
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly ISubscriptionPlanRepository _planRepo;
        private readonly IPaymentService _paymentService;
        private readonly IUserRepository _userRepo;

        public SubscriptionService(
            ISubscriptionRepository subscriptionRepo,
            ISubscriptionPlanRepository planRepo,
            IPaymentService paymentService,
            IUserRepository userRepo)
        {
            _subscriptionRepo = subscriptionRepo;
            _planRepo = planRepo;
            _paymentService = paymentService;
            _userRepo = userRepo;
        }
        public async Task<SubscriptionStatusDto> GetMyStatusAsync(int userId)
        {
            // latest subscription (could be Active or Inactive, we’ll show what exists)
            var latest = await _subscriptionRepo.GetLatestByUserAsync(userId);

            // default empty when no subs
            if (latest == null)
            {
                return new SubscriptionStatusDto
                {
                    Status = "None",
                    BorrowedToday = 0,
                    BorrowedThisMonth = 0,
                    RemainingToday = 0,
                    RemainingThisMonth = 0
                };
            }

            // load plan
            var plan = await _planRepo.GetByIdAsync(latest.SubscriptionPlanID);

            // compute usage from Loans/LoanItems via repo (or DbContext if you prefer)
            // BorrowedToday = number of LoanItems created today under ACTIVE subscription(s)
            // BorrowedThisMonth = number in this calendar month under ACTIVE subscription(s)

            // If you don’t have dedicated methods, here’s a simple approach using your _subscriptionRepo context:
            // We’ll treat "borrow" as LoanItems where ReturnedDate == null (currently out) OR simply count all created today/this month.
            // Adjust to your business rule if needed.

            var now = DateTime.UtcNow;
            var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // Load Active subscription for accurate enforcement; if current latest is inactive, usage is 0/0.
            var active = await _subscriptionRepo.GetActiveByUserAsync(userId);

            int borrowedToday = 0, borrowedThisMonth = 0;
            if (active != null)
            {
                // naive counting by LoanItems under active subscription
                // If you already have LoanRepository, use it; otherwise you can add:
                // LoanItems where Loan.SubscriptionID == active.SubscriptionID and DueDate (or item creation) within ranges.
                // Here we’ll use Transaction/Loan repositories if exposed; if not, add two small methods to LoanRepository.
                // For now, conservative zeros; plug real counts if your LoanRepo has methods.
            }

            var maxPerDay = plan?.MaxPerDay;
            var maxPerMonth = plan?.MaxPerMonth;

            return new SubscriptionStatusDto
            {
                SubscriptionId = latest.SubscriptionID,
                PlanName = plan?.PlanName,
                DurationDays = plan?.DurationDays,
                Price = plan?.Price,
                StartDate = latest.StartDate,
                EndDate = latest.EndDate,
                Status = latest.Status,          // "Active" or "Inactive" (purchase makes it Inactive until payment completes)
                MaxPerDay = maxPerDay,
                MaxPerMonth = maxPerMonth,
                BorrowedToday = borrowedToday,
                BorrowedThisMonth = borrowedThisMonth,
                RemainingToday = maxPerDay.HasValue ? Math.Max(0, maxPerDay.Value - borrowedToday) : 0,
                RemainingThisMonth = maxPerMonth.HasValue ? Math.Max(0, maxPerMonth.Value - borrowedThisMonth) : 0
            };
        }

        public async Task<Transaction> PurchaseAsync(int userId, int planId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found. Please login again.");

            var active = await _subscriptionRepo.GetActiveByUserAsync(userId);
            if (active != null)
                throw new InvalidOperationException("User already has an active subscription");

            var plan = await _planRepo.GetByIdAsync(planId);
            if (plan == null)
                throw new InvalidOperationException("Subscription plan not found");

            var start = DateTime.UtcNow;
            var subscription = new Subscription
            {
                UserID = userId,
                SubscriptionPlanID = plan.SubscriptionPlanID,
                StartDate = start,
                EndDate = start.AddDays(plan.DurationDays),
                Status = "Inactive"
            };

            await _subscriptionRepo.AddAsync(subscription);

            return await _paymentService.CreatePendingTransactionAsync(
                userId, subscription.SubscriptionID, plan.Price);
        }

        public async Task<Transaction> RenewAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found. Please login again.");

            var latest = await _subscriptionRepo.GetLatestByUserAsync(userId);
            if (latest == null)
                throw new InvalidOperationException("No subscription to renew");

            var plan = await _planRepo.GetByIdAsync(latest.SubscriptionPlanID);
            if (plan == null)
                throw new InvalidOperationException("Subscription plan not found");

            var start = latest.EndDate > DateTime.UtcNow ? latest.EndDate : DateTime.UtcNow;
            var subscription = new Subscription
            {
                UserID = userId,
                SubscriptionPlanID = plan.SubscriptionPlanID,
                StartDate = start,
                EndDate = start.AddDays(plan.DurationDays),
                Status = "Inactive"
            };

            await _subscriptionRepo.AddAsync(subscription);

            return await _paymentService.CreatePendingTransactionAsync(
                userId, subscription.SubscriptionID, plan.Price);
        }
    }
}
