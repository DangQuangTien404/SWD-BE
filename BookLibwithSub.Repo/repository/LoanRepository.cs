using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookLibwithSub.Repo.repository
{
    public class LoanRepository : ILoanRepository
    {
        private readonly AppDbContext _context;
        public LoanRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountLoanItemsAsync(int subscriptionId, DateTime start, DateTime end)
        {
            return await _context.LoanItems
                .Where(li => li.Loan.SubscriptionID == subscriptionId &&
                             li.Loan.LoanDate >= start && li.Loan.LoanDate < end)
                .CountAsync();
        }

        public async Task AddAsync(Loan loan)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            foreach (var item in loan.LoanItems)
            {
                var affected = await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE BookID = {item.BookID} AND AvailableCopies > 0");
                if (affected == 0)
                    throw new InvalidOperationException($"Book {item.BookID} not available");
            }

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task<Loan?> GetByIdAsync(int loanId)
        {
            return await _context.Loans
                .Include(l => l.Subscription)
                    .ThenInclude(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(l => l.LoanID == loanId);
        }

        public async Task AddItemsAsync(Loan loan, IEnumerable<LoanItem> items)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            foreach (var item in items)
            {
                var affected = await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE BookID = {item.BookID} AND AvailableCopies > 0");
                if (affected == 0)
                    throw new InvalidOperationException($"Book {item.BookID} not available");
                item.LoanID = loan.LoanID;
                _context.LoanItems.Add(item);
            }
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task<LoanItem> ReturnAsync(int loanItemId)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            var item = await _context.LoanItems
                .Include(li => li.Loan)
                    .ThenInclude(l => l.Subscription)
                .FirstOrDefaultAsync(li => li.LoanItemID == loanItemId);
            if (item == null)
                throw new InvalidOperationException("Loan item not found");
            if (item.Status == "Returned")
                return item;

            item.ReturnedDate = DateTime.UtcNow;
            item.Status = "Returned";

            var affected = await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE BookID = {item.BookID}");
            if (affected == 0)
                throw new InvalidOperationException($"Book {item.BookID} not found");

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return item;
        }

        public async Task<List<Loan>> GetLoansByUserAsync(int userId)
        {
            return await _context.Loans
                .Include(l => l.LoanItems)
                .Where(l => l.Subscription.UserID == userId)
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();
        }

        public async Task<List<Loan>> GetActiveLoansByUserAsync(int userId)
        {
            return await _context.Loans
                .Where(l => l.Subscription.UserID == userId &&
                             l.LoanItems.Any(li => li.Status != "Returned"))
                .Include(l => l.LoanItems.Where(li => li.Status != "Returned"))
                .ToListAsync();
        }
    }
}
