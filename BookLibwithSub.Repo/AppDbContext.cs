using BookLibwithSub.Repo.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLibwithSub.Repo;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<SystemAccount> SystemAccounts => Set<SystemAccount>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanItem> LoanItems => Set<LoanItem>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<BookAuthor>().HasKey(x => new { x.BookId, x.AuthorId });
        b.Entity<BookAuthor>().HasOne(x => x.Book).WithMany(x => x.BookAuthors).HasForeignKey(x => x.BookId);
        b.Entity<BookAuthor>().HasOne(x => x.Author).WithMany(x => x.BookAuthors).HasForeignKey(x => x.AuthorId);

        b.Entity<Inventory>().HasKey(x => x.BookId);
        b.Entity<Inventory>().Property(x => x.RowVersion).IsRowVersion(); 
        b.Entity<Inventory>().HasOne(x => x.Book).WithOne(x => x.Inventory)
            .HasForeignKey<Inventory>(x => x.BookId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<LoanItem>().HasOne(x => x.Loan).WithMany(x => x.Items).HasForeignKey(x => x.LoanId);
        b.Entity<LoanItem>().HasOne(x => x.Book).WithMany(x => x.LoanItems).HasForeignKey(x => x.BookId);

        // --- Subscriptions
        b.Entity<Subscription>().HasOne(x => x.Member).WithMany(x => x.Subscriptions).HasForeignKey(x => x.MemberId);
        b.Entity<Subscription>().HasOne(x => x.Plan).WithMany(x => x.Subscriptions).HasForeignKey(x => x.PlanId);

        b.Entity<Transaction>().HasOne(x => x.Member).WithMany(x => x.Transactions)
            .HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict); 

        b.Entity<Transaction>().HasOne(x => x.Subscription).WithMany()
            .HasForeignKey(x => x.SubscriptionId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Transaction>().HasOne(x => x.LoanItem).WithMany()
            .HasForeignKey(x => x.LoanItemId).OnDelete(DeleteBehavior.SetNull);

        b.Entity<SubscriptionPlan>().Property(x => x.Price).HasPrecision(18, 2);
        b.Entity<LoanItem>().Property(x => x.FineAmount).HasPrecision(18, 2);
        b.Entity<Transaction>().Property(x => x.Amount).HasPrecision(18, 2);

        b.Entity<Member>().HasIndex(x => x.Email).IsUnique();
        b.Entity<SystemAccount>().HasIndex(x => x.Username).IsUnique();

        b.Entity<Transaction>().HasCheckConstraint("CK_Transaction_Type",
            "Type IN ('Payment','Fine','Adjustment')");
        b.Entity<Transaction>().HasCheckConstraint("CK_Transaction_Status",
            "Status IN ('Pending','Success','Failed','Posted')");
    }
}
