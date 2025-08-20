using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Service.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<(IEnumerable<Book> items, int total)> SearchAsync(string? q, int page, int pageSize);
        Task<Book?> GetByIdAsync(int id);
        Task<Book> CreateAsync(Book entity, byte[]? coverBytes, string? contentType);
        Task UpdateAsync(Book entity, byte[]? coverBytes, string? contentType);
        Task DeleteAsync(int id);
    }
}
