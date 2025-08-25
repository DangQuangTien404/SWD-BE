using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookLibwithSub.Repo
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) => _context = context;

        
        public Task<List<User>> GetAllAsync() =>
            _context.Users
                .AsNoTracking()
                .ToListAsync();

        public Task<User?> GetByIdAsync(int userId) =>
            _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserID == userId);

        public Task<User?> GetByUsernameAsync(string username) =>
            _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

        public Task<User?> GetByEmailAsync(string email) =>
            _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

        public Task<User?> GetByTokenAsync(string token) =>
            _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.CurrentToken == token);

        
        public Task<User?> GetTrackedByIdAsync(int userId) =>
            _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

        
        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            
            var entry = _context.Entry(user);
            if (entry.State == EntityState.Detached)
            {
                _context.Users.Attach(user);
                entry.State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int userId)
        {
            var u = await _context.Users.FirstOrDefaultAsync(x => x.UserID == userId);
            if (u == null) return;

            var hasSubs = await _context.Subscriptions.AnyAsync(s => s.UserID == userId);
            var hasTxns = await _context.Transactions.AnyAsync(t => t.UserID == userId);

            if (hasSubs || hasTxns)
                throw new InvalidOperationException("Cannot delete: user has related subscriptions or transactions.");

            _context.Users.Remove(u);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateTokenAsync(int userId, string? token)
        {
            var tracked = await _context.Users.FirstOrDefaultAsync(x => x.UserID == userId);
            if (tracked == null) return;
            tracked.CurrentToken = token;
            await _context.SaveChangesAsync();
        }
    }
}
