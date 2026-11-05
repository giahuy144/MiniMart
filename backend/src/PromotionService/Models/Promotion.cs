namespace PromotionService.Models;

public class Promotion
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiscountType { get; set; } = "Percentage"; // "Percentage" or "FixedAmount"
    public decimal DiscountValue { get; set; } // e.g. 10 (%) or 20000 (VND)
    public decimal? MaxDiscountAmount { get; set; }
    public decimal MinOrderAmount { get; set; } = 0;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    public int UsageLimit { get; set; } = 1000;
    public int UsedCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PromotionProduct> AppliedProducts { get; set; } = new List<PromotionProduct>();
    public ICollection<PromotionUsage> Usages { get; set; } = new List<PromotionUsage>();
}

public class PromotionProduct
{
    public int Id { get; set; }
    public int PromotionId { get; set; }
    public Promotion? Promotion { get; set; }
    public int? ProductId { get; set; }
    public int? CategoryId { get; set; }
}

public class PromotionUsage
{
    public int Id { get; set; }
    public int PromotionId { get; set; }
    public Promotion? Promotion { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Used"; // "Used" or "Reverted"
}
