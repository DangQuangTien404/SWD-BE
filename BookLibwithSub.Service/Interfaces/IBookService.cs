using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Service.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book?> GetByIdAsync(int id);
        Task<Book> CreateAsync(Book entity);
        Task<Book?> UpdateAsync(int id, Book updated);
        Task<bool> DeleteAsync(int id);
    }
}
