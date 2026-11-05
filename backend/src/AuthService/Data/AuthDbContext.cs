using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using MiniMart.Shared.Constants;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(30).IsRequired();
        });

        // Seed initial accounts
        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cashierId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var stockId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var custId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = adminId,
                Username = "admin",
                FullName = "Quản Trị Viên",
                Email = "admin@minimart.vn",
                PhoneNumber = "0900000001",
                PasswordHash = PasswordHasher.HashPassword("admin123"),
                Role = RoleConstants.Admin,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = cashierId,
                Username = "cashier",
                FullName = "Nhân Viên Thu Ngân",
                Email = "cashier@minimart.vn",
                PhoneNumber = "0900000002",
                PasswordHash = PasswordHasher.HashPassword("cashier123"),
                Role = RoleConstants.Cashier,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = stockId,
                Username = "stockkeeper",
                FullName = "Thủ Kho Hàng",
                Email = "stockkeeper@minimart.vn",
                PhoneNumber = "0900000003",
                PasswordHash = PasswordHasher.HashPassword("stock123"),
                Role = RoleConstants.Stockkeeper,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = custId,
                Username = "customer",
                FullName = "Nguyễn Văn Khách",
                Email = "customer@gmail.com",
                PhoneNumber = "0900000004",
                PasswordHash = PasswordHasher.HashPassword("customer123"),
                Role = RoleConstants.Customer,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
