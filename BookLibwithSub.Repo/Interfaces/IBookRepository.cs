using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Repo.Interfaces
{
    public interface IBookRepository
    {
        Task<List<Book>> GetAllAsync();

        Task<(IEnumerable<Book> items, int total)> SearchAsync(string? q, int page, int pageSize);

        Task<IEnumerable<Book>> GetByExactTitleAsync(string title);

        Task<Book?> GetByIdAsync(int id);

        Task<Book> AddAsync(Book entity);
        Task UpdateAsync(Book entity);
        Task DeleteAsync(int id);

        Task<bool> IsIsbnTakenAsync(string isbn, int? exceptBookId = null);
    }
}