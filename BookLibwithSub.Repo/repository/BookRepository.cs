using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookLibwithSub.Repo.repository
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;
        public BookRepository(AppDbContext context) => _context = context;

        public async Task<List<Book>> GetAllAsync()
        {
            return await _context.Books
                .AsNoTracking()
                .OrderBy(b => b.Title)
                .ThenBy(b => b.BookID)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Book> items, int total)> SearchAsync(string? q, int page, int pageSize)
        {
            var query = _context.Books.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(b =>
                    EF.Functions.Like(b.Title, $"%{term}%") ||
                    EF.Functions.Like(b.AuthorName, $"%{term}%") ||
                    EF.Functions.Like(b.ISBN, $"%{term}%"));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<IEnumerable<Book>> GetByExactTitleAsync(string title)
        {
            var t = title.Trim();

            return await _context.Books
                .AsNoTracking()
                .Where(b => b.Title.ToLower() == t.ToLower())
                .OrderBy(b => b.BookID)
                .ToListAsync();
        }

        public Task<Book?> GetByIdAsync(int id) =>
            _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookID == id);

        public async Task<Book> AddAsync(Book entity)
        {

            if (await IsIsbnTakenAsync(entity.ISBN))
                throw new InvalidOperationException($"ISBN '{entity.ISBN}' already exists.");

            _context.Books.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Book entity)
        {
            if (await IsIsbnTakenAsync(entity.ISBN, entity.BookID))
                throw new InvalidOperationException($"ISBN '{entity.ISBN}' already exists.");

            _context.Books.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.Books.FirstOrDefaultAsync(b => b.BookID == id);
            if (existing == null) return;
            _context.Books.Remove(existing);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsIsbnTakenAsync(string isbn, int? exceptBookId = null)
        {
            var q = _context.Books.AsQueryable().Where(b => b.ISBN == isbn);
            if (exceptBookId.HasValue) q = q.Where(b => b.BookID != exceptBookId.Value);
            return await q.AnyAsync();
        }
    }
}
