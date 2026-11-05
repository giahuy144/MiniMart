using Microsoft.EntityFrameworkCore;
using PromotionService.Models;

namespace PromotionService.Data;

public class PromotionDbContext : DbContext
{
    public PromotionDbContext(DbContextOptions<PromotionDbContext> options) : base(options) { }

    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PromotionProduct> PromotionProducts => Set<PromotionProduct>();
    public DbSet<PromotionUsage> PromotionUsages => Set<PromotionUsage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
            entity.Property(e => e.MaxDiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.MinOrderAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PromotionUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
        });

        // Seed sample promotions
        modelBuilder.Entity<Promotion>().HasData(
            new Promotion
            {
                Id = 1,
                Code = "CHAOMUNG",
                Name = "Ưu đãi Chào mừng khách hàng",
                Description = "Giảm 10% tối đa 50,000đ cho đơn hàng từ 50,000đ",
                DiscountType = "Percentage",
                DiscountValue = 10,
                MaxDiscountAmount = 50000,
                MinOrderAmount = 50000,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2027, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                UsageLimit = 500,
                UsedCount = 0,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Promotion
            {
                Id = 2,
                Code = "GIAM20K",
                Name = "Giảm ngay 20,000đ",
                Description = "Giảm trực tiếp 20,000đ cho đơn hàng từ 100,000đ",
                DiscountType = "FixedAmount",
                DiscountValue = 20000,
                MaxDiscountAmount = 20000,
                MinOrderAmount = 100000,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2027, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                UsageLimit = 200,
                UsedCount = 0,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
