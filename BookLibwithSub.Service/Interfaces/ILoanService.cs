using System;
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
        Task<Loan?> GetLoanAsync(int loanId, int userId);
        Task ExtendLoanAsync(int loanId, int userId, DateTime? newDueDate, int? daysToExtend);
        Task<IEnumerable<Loan>> GetLoanHistoryAsync(int userId);
        Task<IEnumerable<Loan>> GetActiveLoansAsync(int userId);
    }
}
