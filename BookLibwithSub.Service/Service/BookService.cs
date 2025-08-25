using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using BookLibwithSub.Service.Interfaces;

namespace BookLibwithSub.Service.Service
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _repo;
        public BookService(IBookRepository repo) => _repo = repo;

        public Task<IEnumerable<Book>> GetAllAsync() => _repo.GetAllAsync();

        public Task<Book?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<Book> CreateAsync(Book entity)
        {
            
            if (await _repo.ExistsByIsbnAsync(entity.ISBN))
                throw new System.InvalidOperationException("ISBN already exists");

            if (entity.TotalCopies < 0 || entity.AvailableCopies < 0 || entity.AvailableCopies > entity.TotalCopies)
                throw new System.InvalidOperationException("Invalid copies numbers");

            await _repo.AddAsync(entity);
            await _repo.SaveAsync();
            return entity;
        }

        public async Task<Book?> UpdateAsync(int id, Book updated)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            if (!string.Equals(existing.ISBN, updated.ISBN) &&
                await _repo.ExistsByIsbnAsync(updated.ISBN, excludeId: id))
                throw new System.InvalidOperationException("ISBN already exists");

            if (updated.TotalCopies < 0 || updated.AvailableCopies < 0 || updated.AvailableCopies > updated.TotalCopies)
                throw new System.InvalidOperationException("Invalid copies numbers");

            
            existing.Title = updated.Title;
            existing.AuthorName = updated.AuthorName;
            existing.ISBN = updated.ISBN;
            existing.Publisher = updated.Publisher;
            existing.PublishedYear = updated.PublishedYear;
            existing.TotalCopies = updated.TotalCopies;
            existing.AvailableCopies = updated.AvailableCopies;
            existing.CoverImageUrl = updated.CoverImageUrl; 

            await _repo.UpdateAsync(existing);
            await _repo.SaveAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;
            await _repo.DeleteAsync(existing);
            await _repo.SaveAsync();
            return true;
        }
    }
}
