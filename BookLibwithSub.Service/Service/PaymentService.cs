using System;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using BookLibwithSub.Service.Interfaces;

namespace BookLibwithSub.Service.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly ISubscriptionRepository _subscriptionRepo;

        public PaymentService(ITransactionRepository transactionRepo, ISubscriptionRepository subscriptionRepo)
        {
            _transactionRepo = transactionRepo;
            _subscriptionRepo = subscriptionRepo;
        }

        public async Task<Transaction> CreatePendingTransactionAsync(int userId, int subscriptionId, decimal amount)
        {
            var transaction = new Transaction
            {
                UserID = userId,
                SubscriptionID = subscriptionId,
                Amount = amount,
                TransactionType = "Subscription",
                TransactionDate = DateTime.UtcNow,
                Status = "Pending"
            };

            await _transactionRepo.AddAsync(transaction);
            return transaction;
        }

        public async Task MarkTransactionPaidAsync(int transactionId)
        {
            var transaction = await _transactionRepo.GetByIdAsync(transactionId);
            if (transaction == null) return;

            transaction.Status = "Paid";
            await _transactionRepo.UpdateAsync(transaction);

            if (transaction.SubscriptionID.HasValue)
            {
                var subscription = await _subscriptionRepo.GetByIdWithPlanAsync(transaction.SubscriptionID.Value);
                if (subscription != null)
                {
                    subscription.Status = "Active";
                    await _subscriptionRepo.UpdateAsync(subscription);
                }
            }
        }

        public async Task RecordFineAsync(int userId, int loanItemId, decimal amount)
        {
            var transaction = new Transaction
            {
                UserID = userId,
                Amount = amount,
                TransactionType = "Fine",
                TransactionDate = DateTime.UtcNow,
                Status = "Unpaid"
            };

            await _transactionRepo.AddAsync(transaction);
        }
    }
}
