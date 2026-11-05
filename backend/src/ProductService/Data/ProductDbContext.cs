using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.Property(e => e.Barcode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CostPrice).HasPrecision(18, 2);
            entity.Property(e => e.SellingPrice).HasPrecision(18, 2);
            entity.Property(e => e.Unit).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired();

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Supplier)
                  .WithMany(s => s.Products)
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Seed data
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Sữa & Sản phẩm từ sữa", Description = "Sữa chua, sữa tươi tiệt trùng, bơ phô mai" },
            new Category { Id = 2, Name = "Mì ăn liền & Đồ khô", Description = "Mì, miến, phở ăn liền, bún khô" },
            new Category { Id = 3, Name = "Nước giải khát", Description = "Nước ngọt, nước suối, trà thảo mộc" },
            new Category { Id = 4, Name = "Bánh kẹo & Ăn vặt", Description = "Bánh quy, bim bim, sô cô la" },
            new Category { Id = 5, Name = "Gia vị & Đồ hộp", Description = "Dầu ăn, nước mắm, sốt tương" }
        );

        modelBuilder.Entity<Supplier>().HasData(
            new Supplier { Id = 1, Name = "Công ty CP Sữa Việt Nam (Vinamilk)", ContactPerson = "Nguyễn Văn Vin", PhoneNumber = "02854155555", Email = "contact@vinamilk.com.vn", Address = "Quận 7, TP.HCM" },
            new Supplier { Id = 2, Name = "Acecook Việt Nam", ContactPerson = "Trần Ace", PhoneNumber = "02838154064", Email = "info@acecookvietnam.com", Address = "KCN Tân Bình, TP.HCM" },
            new Supplier { Id = 3, Name = "Suntory PepsiCo Việt Nam", ContactPerson = "Lê Pepsi", PhoneNumber = "02838219490", Email = "pepsi@pepsico.com.vn", Address = "Quận 1, TP.HCM" },
            new Supplier { Id = 4, Name = "Orion Food Vina", ContactPerson = "Phạm Orion", PhoneNumber = "02837761000", Email = "orion@orion.vn", Address = "Bình Dương" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Barcode = "8934673123456",
                Sku = "SUA-VNM-180",
                Name = "Sữa tươi tiệt trùng Vinamilk có đường 180ml",
                Description = "Lốc 4 hộp sữa tươi tiệt trùng Vinamilk thơm ngon bổ dưỡng",
                Unit = "Hộp",
                CostPrice = 7500,
                SellingPrice = 10000,
                ImageUrl = "https://images.unsplash.com/photo-1550583724-b2692b85b150?w=400",
                CategoryId = 1,
                SupplierId = 1,
                Status = "Active",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 2,
                Barcode = "8934567890123",
                Sku = "MI-HAOHAO-TC",
                Name = "Mì Hảo Hảo tôm chua cay 75g",
                Description = "Mì ăn liền Hảo Hảo hương vị tôm chua cay truyền thống",
                Unit = "Gói",
                CostPrice = 3500,
                SellingPrice = 5000,
                ImageUrl = "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400",
                CategoryId = 2,
                SupplierId = 2,
                Status = "Active",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 3,
                Barcode = "8935001700012",
                Sku = "NG-COCA-330",
                Name = "Nước ngọt Coca-Cola lon 330ml",
                Description = "Nước giải khát có ga Coca-Cola sảng khoái bùng nổ",
                Unit = "Lon",
                CostPrice = 7000,
                SellingPrice = 11000,
                ImageUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=400",
                CategoryId = 3,
                SupplierId = 3,
                Status = "Active",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 4,
                Barcode = "8934822010101",
                Sku = "BK-CHOCOPIE-12",
                Name = "Bánh Chocopie Orion hộp 12 cái 360g",
                Description = "Bánh xốp phủ sô-cô-la nhân kem dẻo mềm béo thơm",
                Unit = "Hộp",
                CostPrice = 42000,
                SellingPrice = 55000,
                ImageUrl = "https://images.unsplash.com/photo-1548848221-0c2e497ed557?w=400",
                CategoryId = 4,
                SupplierId = 4,
                Status = "Active",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 5,
                Barcode = "8935049500010",
                Sku = "NS-AQUAFINA-500",
                Name = "Nước tinh khiết Aquafina chai 500ml",
                Description = "Nước uống đóng chai tinh khiết chuẩn chất lượng quốc tế",
                Unit = "Chai",
                CostPrice = 3000,
                SellingPrice = 6000,
                ImageUrl = "https://images.unsplash.com/photo-1560023907-5f339617ea30?w=400",
                CategoryId = 3,
                SupplierId = 3,
                Status = "Active",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
