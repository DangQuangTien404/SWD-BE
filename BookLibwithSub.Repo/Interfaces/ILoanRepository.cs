using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Repo.Interfaces
{
    public interface ILoanRepository
    {
        Task<int> CountLoanItemsAsync(int subscriptionId, DateTime start, DateTime end);
        Task AddAsync(Loan loan);
        Task<Loan?> GetByIdAsync(int loanId);
        Task AddItemsAsync(Loan loan, IEnumerable<LoanItem> items);
        Task<LoanItem> ReturnAsync(int loanItemId);
        Task<List<Loan>> GetLoansByUserAsync(int userId);
        Task<List<Loan>> GetActiveLoansByUserAsync(int userId);
    }
}
