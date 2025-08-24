using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookLibwithSub.Repo.repository
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _db;
        public BookRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<Book>> GetAllAsync() =>
            await _db.Books.AsNoTracking()
                           .OrderBy(b => b.Title)
                           .ToListAsync();

        public Task<Book?> GetByIdAsync(int id) =>
            _db.Books.FirstOrDefaultAsync(b => b.BookID == id);

        public async Task<bool> ExistsByIsbnAsync(string isbn, int? excludeId = null)
        {
            var q = _db.Books.AsNoTracking().Where(b => b.ISBN == isbn);
            if (excludeId.HasValue) q = q.Where(b => b.BookID != excludeId.Value);
            return await q.AnyAsync();
        }

        public async Task AddAsync(Book entity) => await _db.Books.AddAsync(entity);

        public Task UpdateAsync(Book entity)
        {
            _db.Books.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Book entity)
        {
            _db.Books.Remove(entity);
            return Task.CompletedTask;
        }

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}
