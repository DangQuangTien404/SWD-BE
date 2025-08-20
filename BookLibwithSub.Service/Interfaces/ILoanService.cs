using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Service.Interfaces
{
    public interface ILoanService
    {
        Task BorrowAsync(int subscriptionId, IEnumerable<int> bookIds);
        Task AddItemsAsync(int loanId, IEnumerable<int> bookIds);
        Task ReturnAsync(int loanItemId);
        Task<IEnumerable<Loan>> GetLoanHistoryAsync(int userId);
        Task<IEnumerable<Loan>> GetActiveLoansAsync(int userId);
    }
}
