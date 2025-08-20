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

        // ---------- READS: No tracking for query-only paths ----------
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

        // ---------- READ (Tracked): for update scenarios ----------
        public Task<User?> GetTrackedByIdAsync(int userId) =>
            _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

        // ---------- WRITES ----------
        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            // If the entity is detached, attach then mark modified
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
