using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using BookLibwithSub.Service.Interfaces;

namespace BookLibwithSub.Service.Service
{
    public class LoanService : ILoanService
    {
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly ILoanRepository _loanRepo;
        private readonly IPaymentService _paymentService;

        public LoanService(ISubscriptionRepository subscriptionRepo, ILoanRepository loanRepo, IPaymentService paymentService)
        {
            _subscriptionRepo = subscriptionRepo;
            _loanRepo = loanRepo;
            _paymentService = paymentService;
        }

        public async Task BorrowAsync(int subscriptionId, IEnumerable<int> bookIds)
        {
            var subscription = await _subscriptionRepo.GetByIdWithPlanAsync(subscriptionId);
            if (subscription == null)
                throw new InvalidOperationException("Subscription not found");

            if (subscription.Status != "Active" ||
                subscription.StartDate > DateTime.UtcNow ||
                subscription.EndDate < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Subscription is not active or out of date range");
            }

            var plan = subscription.SubscriptionPlan;
            if (plan == null)
                throw new InvalidOperationException("Subscription plan missing");

            var now = DateTime.UtcNow;
            var dayStart = now.Date;
            var dayEnd = dayStart.AddDays(1);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            int alreadyToday = await _loanRepo.CountLoanItemsAsync(subscriptionId, dayStart, dayEnd);
            int alreadyMonth = await _loanRepo.CountLoanItemsAsync(subscriptionId, monthStart, monthEnd);
            int requested = bookIds.Count();

            if (alreadyToday + requested > plan.MaxPerDay)
                throw new InvalidOperationException("Daily borrowing limit exceeded");

            if (alreadyMonth + requested > plan.MaxPerMonth)
                throw new InvalidOperationException("Monthly borrowing limit exceeded");

            var loan = new Loan
            {
                SubscriptionID = subscriptionId,
                LoanDate = now,
                Status = "Borrowed",
                LoanItems = bookIds.Select(id => new LoanItem
                {
                    BookID = id,
                    DueDate = now.AddDays(14),
                    Status = "Borrowed"
                }).ToList()
            };

            await _loanRepo.AddAsync(loan);
        }

        public async Task AddItemsAsync(int loanId, IEnumerable<int> bookIds)
        {
            var loan = await _loanRepo.GetByIdAsync(loanId);
            if (loan == null)
                throw new InvalidOperationException("Loan not found");

            var subscription = loan.Subscription;
            if (subscription == null)
                throw new InvalidOperationException("Subscription not found");

            if (subscription.Status != "Active" ||
                subscription.StartDate > DateTime.UtcNow ||
                subscription.EndDate < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Subscription is not active or out of date range");
            }

            var plan = subscription.SubscriptionPlan;
            if (plan == null)
                throw new InvalidOperationException("Subscription plan missing");

            var now = DateTime.UtcNow;
            var dayStart = now.Date;
            var dayEnd = dayStart.AddDays(1);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            int alreadyToday = await _loanRepo.CountLoanItemsAsync(subscription.SubscriptionID, dayStart, dayEnd);
            int alreadyMonth = await _loanRepo.CountLoanItemsAsync(subscription.SubscriptionID, monthStart, monthEnd);
            int requested = bookIds.Count();

            if (alreadyToday + requested > plan.MaxPerDay)
                throw new InvalidOperationException("Daily borrowing limit exceeded");

            if (alreadyMonth + requested > plan.MaxPerMonth)
                throw new InvalidOperationException("Monthly borrowing limit exceeded");

            var items = bookIds.Select(id => new LoanItem
            {
                BookID = id,
                DueDate = now.AddDays(14),
                Status = "Borrowed"
            }).ToList();

            await _loanRepo.AddItemsAsync(loan, items);
        }

        public async Task ReturnAsync(int loanItemId)
        {
            var item = await _loanRepo.ReturnAsync(loanItemId);

            if (item.ReturnedDate.HasValue && item.ReturnedDate.Value > item.DueDate)
            {
                var daysLate = (item.ReturnedDate.Value.Date - item.DueDate.Date).Days;
                if (daysLate > 0)
                {
                    var fineAmount = daysLate * 1m;
                    var userId = item.Loan?.Subscription?.UserID ?? 0;
                    if (userId != 0)
                    {
                        await _paymentService.RecordFineAsync(userId, item.LoanItemID, fineAmount);
                    }
                }
            }
        }

        public async Task<Loan?> GetLoanAsync(int loanId, int userId)
        {
            var loan = await _loanRepo.GetByIdAsync(loanId);
            if (loan == null || loan.Subscription?.UserID != userId)
                return null;
            return loan;
        }

        public async Task ExtendLoanAsync(int loanId, int userId, DateTime? newDueDate, int? daysToExtend)
        {
            if (!newDueDate.HasValue && !daysToExtend.HasValue)
                throw new ArgumentException("Either new due date or days to extend must be provided");

            var loan = await _loanRepo.GetByIdAsync(loanId);
            if (loan == null || loan.Subscription?.UserID != userId)
                throw new InvalidOperationException("Loan not found");

            await _loanRepo.ExtendLoanAsync(loan, newDueDate, daysToExtend);
        }

        public async Task<IEnumerable<Loan>> GetLoanHistoryAsync(int userId)
        {
            return await _loanRepo.GetLoansByUserAsync(userId);
        }

        public async Task<IEnumerable<Loan>> GetActiveLoansAsync(int userId)
        {
            return await _loanRepo.GetActiveLoansByUserAsync(userId);
        }
    }
}
