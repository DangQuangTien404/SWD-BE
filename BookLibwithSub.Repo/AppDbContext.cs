using Microsoft.EntityFrameworkCore;
using BookLibwithSub.Repo.Entities;

namespace BookLibwithSub.Repo
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanItem> LoanItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {

                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();

                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(u => u.FullName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(u => u.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(u => u.Role)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.CurrentToken)
                    .HasMaxLength(500);

                entity.Property(u => u.CreatedDate)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {

                entity.Property(p => p.PlanName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.DurationDays)
                    .IsRequired();

                entity.Property(p => p.MaxPerDay)
                    .IsRequired();

                entity.Property(p => p.MaxPerMonth)
                    .IsRequired();

                entity.Property(p => p.Price)
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_SubscriptionPlan_Quotas",
                        "\"DurationDays\" > 0 AND \"MaxPerDay\" >= 0 AND \"MaxPerMonth\" >= 0");
                });
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasOne(s => s.User)
                    .WithMany(u => u.Subscriptions)
                    .HasForeignKey(s => s.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.SubscriptionPlan)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(s => s.SubscriptionPlanID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(s => s.StartDate).IsRequired();
                entity.Property(s => s.EndDate).IsRequired();
                entity.Property(s => s.Status)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.Property(b => b.Title)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(b => b.AuthorName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(b => b.ISBN)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(b => b.CoverImage)
                      .HasColumnType("bytea");

                entity.Property(b => b.CoverImageContentType)
                       .HasMaxLength(100);

                entity.Property(b => b.Publisher)
                    .HasMaxLength(255);

                entity.Property(b => b.PublishedYear)
                    .IsRequired();

                entity.Property(b => b.TotalCopies)
                    .IsRequired();

                entity.Property(b => b.AvailableCopies)
                    .IsRequired();

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Book_Copies",
                        "\"TotalCopies\" >= 0 AND \"AvailableCopies\" >= 0 AND \"AvailableCopies\" <= \"TotalCopies\"");
                });
            });

            modelBuilder.Entity<Loan>(entity =>
            {
                entity.HasOne(l => l.Subscription)
                    .WithMany(s => s.Loans)
                    .HasForeignKey(l => l.SubscriptionID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(l => l.LoanDate).IsRequired();
                entity.Property(l => l.ReturnDate).IsRequired(false);
                entity.Property(l => l.Status)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<LoanItem>(entity =>
            {
                entity.HasOne(li => li.Loan)
                    .WithMany(l => l.LoanItems)
                    .HasForeignKey(li => li.LoanID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(li => li.Book)
                    .WithMany(b => b.LoanItems)
                    .HasForeignKey(li => li.BookID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(li => li.DueDate).IsRequired();
                entity.Property(li => li.ReturnedDate).IsRequired(false);
                entity.Property(li => li.Status)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(li => new { li.LoanID, li.BookID });
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasOne(t => t.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(t => t.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Subscription)
                    .WithMany(s => s.Transactions)
                    .HasForeignKey(t => t.SubscriptionID)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(t => t.TransactionType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(t => t.Amount)
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();

                entity.Property(t => t.TransactionDate)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(t => t.Status)
                    .IsRequired()
                    .HasMaxLength(50);
            });
        }
    }
}
