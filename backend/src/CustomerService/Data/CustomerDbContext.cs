using CustomerService.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Data;

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PointHistory> PointHistories => Set<PointHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.Property(e => e.FullName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Tier).HasMaxLength(30).IsRequired();
            entity.Property(e => e.TotalSpent).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PointHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionType).HasMaxLength(30).IsRequired();
            entity.Property(e => e.ReferenceId).HasMaxLength(50);
        });

        // Seed initial customer corresponding to seed user customer in AuthService
        var defaultCustUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = 1,
                UserId = defaultCustUserId,
                FullName = "Nguyễn Văn Khách",
                PhoneNumber = "0900000004",
                Email = "customer@gmail.com",
                Address = "170 An Dương Vương, TP. Quy Nhơn",
                Points = 120,
                TotalSpent = 1200000,
                Tier = "Bạc",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Customer
            {
                Id = 2,
                UserId = null,
                FullName = "Trần Thị Mai (Khách vãng lai)",
                PhoneNumber = "0987654321",
                Email = "mai.tran@gmail.com",
                Address = "TP. Quy Nhơn",
                Points = 35,
                TotalSpent = 350000,
                Tier = "Thường",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
