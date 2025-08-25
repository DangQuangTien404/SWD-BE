
using System;
using BookLibwithSub.Repo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BookLibwithSub.Repo.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250821095235_Init_V1")]
    partial class Init_V1
    {
        
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.19")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.Book", b =>
                {
                    b.Property<int>("BookID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("BookID"));

                    b.Property<string>("AuthorName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("AvailableCopies")
                        .HasColumnType("integer");

                    b.Property<string>("CoverImageUrl")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("ISBN")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<int>("PublishedYear")
                        .HasColumnType("integer");

                    b.Property<string>("Publisher")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("TotalCopies")
                        .HasColumnType("integer");

                    b.HasKey("BookID");

                    b.HasIndex("ISBN")
                        .IsUnique();

                    b.ToTable("Books", t =>
                        {
                            t.HasCheckConstraint("CK_Book_Copies", "\"TotalCopies\" >= 0 AND \"AvailableCopies\" >= 0 AND \"AvailableCopies\" <= \"TotalCopies\"");
                        });
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.Loan", b =>
                {
                    b.Property<int>("LoanID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("LoanID"));

                    b.Property<DateTime>("LoanDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ReturnDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("SubscriptionID")
                        .HasColumnType("integer");

                    b.HasKey("LoanID");

                    b.HasIndex("SubscriptionID");

                    b.ToTable("Loans");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.LoanItem", b =>
                {
                    b.Property<int>("LoanItemID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("LoanItemID"));

                    b.Property<int>("BookID")
                        .HasColumnType("integer");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("LoanID")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("ReturnedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("LoanItemID");

                    b.HasIndex("BookID");

                    b.HasIndex("LoanID", "BookID");

                    b.ToTable("LoanItems");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.Subscription", b =>
                {
                    b.Property<int>("SubscriptionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("SubscriptionID"));

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("SubscriptionPlanID")
                        .HasColumnType("integer");

                    b.Property<int>("UserID")
                        .HasColumnType("integer");

                    b.HasKey("SubscriptionID");

                    b.HasIndex("SubscriptionPlanID");

                    b.HasIndex("UserID");

                    b.ToTable("Subscriptions");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.SubscriptionPlan", b =>
                {
                    b.Property<int>("SubscriptionPlanID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("SubscriptionPlanID"));

                    b.Property<int>("DurationDays")
                        .HasColumnType("integer");

                    b.Property<int>("MaxPerDay")
                        .HasColumnType("integer");

                    b.Property<int>("MaxPerMonth")
                        .HasColumnType("integer");

                    b.Property<string>("PlanName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric(18,2)");

                    b.HasKey("SubscriptionPlanID");

                    b.ToTable("SubscriptionPlans", t =>
                        {
                            t.HasCheckConstraint("CK_SubscriptionPlan_Quotas", "\"DurationDays\" > 0 AND \"MaxPerDay\" >= 0 AND \"MaxPerMonth\" >= 0");
                        });
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.Transaction", b =>
                {
                    b.Property<int>("TransactionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TransactionID"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric(18,2)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int?>("SubscriptionID")
                        .HasColumnType("integer");

                    b.Property<DateTime>("TransactionDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("TransactionType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("UserID")
                        .HasColumnType("integer");

                    b.HasKey("TransactionID");

                    b.HasIndex("SubscriptionID");

                    b.HasIndex("UserID");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.User", b =>
                {
                    b.Property<int>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("UserID"));

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("CurrentToken")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("UserID");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.Loan", b =>
                {
                    b.HasOne("BookLibwithSub.Repo.Entities.Subscription", "Subscription")
                        .WithMany("Loans")
                        .HasForeignKey("SubscriptionID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Subscription");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.LoanItem", b =>
                {
                    b.HasOne("BookLibwithSub.Repo.Entities.Book", "Book")
                        .WithMany("LoanItems")
                        .HasForeignKey("BookID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("BookLibwithSub.Repo.Entities.Loan", "Loan")
                        .WithMany("LoanItems")
                        .HasForeignKey("LoanID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");

                    b.Navigation("Loan");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.Subscription", b =>
                {
                    b.HasOne("BookLibwithSub.Repo.Entities.SubscriptionPlan", "SubscriptionPlan")
                        .WithMany("Subscriptions")
                        .HasForeignKey("SubscriptionPlanID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("BookLibwithSub.Repo.Entities.User", "User")
                        .WithMany("Subscriptions")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("SubscriptionPlan");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.Transaction", b =>
                {
                    b.HasOne("BookLibwithSub.Repo.Entities.Subscription", "Subscription")
                        .WithMany("Transactions")
                        .HasForeignKey("SubscriptionID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("BookLibwithSub.Repo.Entities.User", "User")
                        .WithMany("Transactions")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Subscription");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.Book", b =>
                {
                    b.Navigation("LoanItems");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.Loan", b =>
                {
                    b.Navigation("LoanItems");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.Subscription", b =>
                {
                    b.Navigation("Loans");

                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.SubscriptionPlan", b =>
                {
                    b.Navigation("Subscriptions");
                });

            modelBuilder.Entity("BookLibwithSub.Repo.Entities.User", b =>
                {
                    b.Navigation("Subscriptions");

                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
