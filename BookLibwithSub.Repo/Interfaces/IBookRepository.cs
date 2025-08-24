using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Repo.Interfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book?> GetByIdAsync(int id);
        Task<bool> ExistsByIsbnAsync(string isbn, int? excludeId = null);
        Task AddAsync(Book entity);
        Task UpdateAsync(Book entity);
        Task DeleteAsync(Book entity);
        Task SaveAsync();
    }
}
