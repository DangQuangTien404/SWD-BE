using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Service.Interfaces
{
    public interface IPaymentService
    {
        Task<Transaction> CreatePendingTransactionAsync(int userId, int subscriptionId, decimal amount);
        Task MarkTransactionPaidAsync(int transactionId);
        Task RecordFineAsync(int userId, int loanItemId, decimal amount);
    }
}
