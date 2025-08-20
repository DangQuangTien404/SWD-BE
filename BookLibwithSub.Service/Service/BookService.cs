using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using BookLibwithSub.Service.Interfaces;

namespace BookLibwithSub.Service.Service
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _repo;
        public BookService(IBookRepository repo) => _repo = repo;

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public Task<(IEnumerable<Book> items, int total)> SearchAsync(string? q, int page, int pageSize)
            => _repo.SearchAsync(q, page, pageSize);

        public Task<Book?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<Book> CreateAsync(Book entity, byte[]? coverBytes, string? contentType)
        {
            if (coverBytes is { Length: > 0 })
            {
                entity.CoverImage = coverBytes;
                entity.CoverImageContentType = string.IsNullOrWhiteSpace(contentType)
                    ? "application/octet-stream" : contentType;
            }
            return await _repo.AddAsync(entity);
        }

        public async Task UpdateAsync(Book entity, byte[]? coverBytes, string? contentType)
        {
            if (coverBytes is { Length: > 0 })
            {
                entity.CoverImage = coverBytes;
                entity.CoverImageContentType = string.IsNullOrWhiteSpace(contentType)
                    ? "application/octet-stream" : contentType;
            }
            await _repo.UpdateAsync(entity);
        }

        public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
    }
}
