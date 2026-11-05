using System.ComponentModel.DataAnnotations;

namespace PromotionService.DTOs;

public class PromotionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; }
}

public class CalculatePromotionRequestDto
{
    [Required]
    public string PromoCode { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal OrderTotal { get; set; }

    public string? CustomerId { get; set; }
}

public class CalculatePromotionResultDto
{
    public bool IsValid { get; set; }
    public int? PromotionId { get; set; }
    public string PromoCode { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal FinalTotal { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class RecordUsageRequestDto
{
    [Required]
    public string PromoCode { get; set; } = string.Empty;

    [Required]
    public string OrderId { get; set; } = string.Empty;

    public string? CustomerId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }
}

public class RollbackUsageRequestDto
{
    [Required]
    public string PromoCode { get; set; } = string.Empty;

    [Required]
    public string OrderId { get; set; } = string.Empty;
}
