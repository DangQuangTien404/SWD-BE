using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Service.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetUserTransactionsAsync(int userId);
    }
}

