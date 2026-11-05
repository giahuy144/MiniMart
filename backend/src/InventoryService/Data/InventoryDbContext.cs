using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<ImportReceipt> ImportReceipts => Set<ImportReceipt>();
    public DbSet<ImportReceiptDetail> ImportReceiptDetails => Set<ImportReceiptDetail>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProductId).IsUnique();
            entity.Property(e => e.Barcode).HasMaxLength(50);
            entity.Property(e => e.ProductName).HasMaxLength(200);
        });

        modelBuilder.Entity<ImportReceipt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceiptNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity.HasMany(e => e.Details)
                  .WithOne(d => d.ImportReceipt)
                  .HasForeignKey(d => d.ImportReceiptId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ImportReceiptDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImportPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionType).HasMaxLength(30).IsRequired();
            entity.Property(e => e.ReferenceId).HasMaxLength(50);
        });

        // Seed stock for sample products
        modelBuilder.Entity<Stock>().HasData(
            new Stock { Id = 1, ProductId = 1, Barcode = "8934673123456", ProductName = "Sữa tươi tiệt trùng Vinamilk có đường 180ml", Quantity = 50, MinStockLevel = 10, LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Stock { Id = 2, ProductId = 2, Barcode = "8934567890123", ProductName = "Mì Hảo Hảo tôm chua cay 75g", Quantity = 100, MinStockLevel = 20, LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Stock { Id = 3, ProductId = 3, Barcode = "8935001700012", ProductName = "Nước ngọt Coca-Cola lon 330ml", Quantity = 40, MinStockLevel = 15, LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Stock { Id = 4, ProductId = 4, Barcode = "8934822010101", ProductName = "Bánh Chocopie Orion hộp 12 cái 360g", Quantity = 5, MinStockLevel = 10, LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }, // Low stock alert!
            new Stock { Id = 5, ProductId = 5, Barcode = "8935049500010", ProductName = "Nước tinh khiết Aquafina chai 500ml", Quantity = 60, MinStockLevel = 15, LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
