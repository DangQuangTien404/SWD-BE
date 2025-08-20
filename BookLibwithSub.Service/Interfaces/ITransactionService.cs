using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Service.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetUserTransactionsAsync(int userId);
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<Transaction?> GetTransactionByIdAsync(int id);
    }
}

