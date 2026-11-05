using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Shared.Responses;
using PromotionService.Data;
using PromotionService.DTOs;
using PromotionService.Models;

namespace PromotionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionsController : ControllerBase
{
    private readonly PromotionDbContext _context;

    public PromotionsController(PromotionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PromotionDto>>>> GetPromotions([FromQuery] bool? activeOnly)
    {
        var query = _context.Promotions.AsQueryable();
        if (activeOnly == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now && p.UsedCount < p.UsageLimit);
        }

        var list = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PromotionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                MaxDiscountAmount = p.MaxDiscountAmount,
                MinOrderAmount = p.MinOrderAmount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                UsageLimit = p.UsageLimit,
                UsedCount = p.UsedCount,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return Ok(ApiResponse<List<PromotionDto>>.Ok(list));
    }

    [HttpGet("code/{code}")]
    public async Task<ActionResult<ApiResponse<PromotionDto>>> GetPromotionByCode(string code)
    {
        var promo = await _context.Promotions.FirstOrDefaultAsync(p => p.Code.ToLower() == code.Trim().ToLower());
        if (promo == null)
        {
            return NotFound(ApiResponse<PromotionDto>.Fail($"Không tìm thấy mã khuyến mãi '{code}'."));
        }

        var dto = new PromotionDto
        {
            Id = promo.Id,
            Code = promo.Code,
            Name = promo.Name,
            Description = promo.Description,
            DiscountType = promo.DiscountType,
            DiscountValue = promo.DiscountValue,
            MaxDiscountAmount = promo.MaxDiscountAmount,
            MinOrderAmount = promo.MinOrderAmount,
            StartDate = promo.StartDate,
            EndDate = promo.EndDate,
            UsageLimit = promo.UsageLimit,
            UsedCount = promo.UsedCount,
            IsActive = promo.IsActive
        };

        return Ok(ApiResponse<PromotionDto>.Ok(dto));
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<ApiResponse<CalculatePromotionResultDto>>> CalculateDiscount([FromBody] CalculatePromotionRequestDto request)
    {
        var promo = await _context.Promotions.FirstOrDefaultAsync(p => p.Code.ToLower() == request.PromoCode.Trim().ToLower());
        if (promo == null)
        {
            return BadRequest(ApiResponse<CalculatePromotionResultDto>.Fail($"Mã khuyến mãi '{request.PromoCode}' không tồn tại."));
        }

        var now = DateTime.UtcNow;
        if (!promo.IsActive || now < promo.StartDate || now > promo.EndDate)
        {
            return BadRequest(ApiResponse<CalculatePromotionResultDto>.Fail("Mã khuyến mãi đã hết hạn hoặc chưa kích hoạt."));
        }

        if (promo.UsedCount >= promo.UsageLimit)
        {
            return BadRequest(ApiResponse<CalculatePromotionResultDto>.Fail("Mã khuyến mãi đã hết lượt sử dụng."));
        }

        if (request.OrderTotal < promo.MinOrderAmount)
        {
            return BadRequest(ApiResponse<CalculatePromotionResultDto>.Fail($"Đơn hàng cần đạt tối thiểu {promo.MinOrderAmount:N0}đ để áp dụng mã này."));
        }

        decimal discount = 0;
        if (promo.DiscountType == "Percentage")
        {
            discount = request.OrderTotal * (promo.DiscountValue / 100m);
            if (promo.MaxDiscountAmount.HasValue && discount > promo.MaxDiscountAmount.Value)
            {
                discount = promo.MaxDiscountAmount.Value;
            }
        }
        else // FixedAmount
        {
            discount = promo.DiscountValue;
            if (discount > request.OrderTotal)
            {
                discount = request.OrderTotal;
            }
        }

        var final = Math.Max(0, request.OrderTotal - discount);

        return Ok(ApiResponse<CalculatePromotionResultDto>.Ok(new CalculatePromotionResultDto
        {
            IsValid = true,
            PromotionId = promo.Id,
            PromoCode = promo.Code,
            DiscountAmount = discount,
            FinalTotal = final,
            Message = $"Áp dụng thành công, giảm {discount:N0}đ"
        }));
    }

    [HttpPost("record-usage")]
    public async Task<ActionResult<ApiResponse<bool>>> RecordUsage([FromBody] RecordUsageRequestDto request)
    {
        var promo = await _context.Promotions.FirstOrDefaultAsync(p => p.Code.ToLower() == request.PromoCode.Trim().ToLower());
        if (promo != null)
        {
            promo.UsedCount += 1;
            _context.PromotionUsages.Add(new PromotionUsage
            {
                PromotionId = promo.Id,
                OrderId = request.OrderId,
                CustomerId = request.CustomerId,
                DiscountAmount = request.DiscountAmount,
                UsedAt = DateTime.UtcNow,
                Status = "Used"
            });
            await _context.SaveChangesAsync();
        }

        return Ok(ApiResponse<bool>.Ok(true, "Ghi nhận lượt khuyến mãi thành công."));
    }

    [HttpPost("rollback-usage")]
    public async Task<ActionResult<ApiResponse<bool>>> RollbackUsage([FromBody] RollbackUsageRequestDto request)
    {
        var promo = await _context.Promotions.FirstOrDefaultAsync(p => p.Code.ToLower() == request.PromoCode.Trim().ToLower());
        if (promo != null && promo.UsedCount > 0)
        {
            promo.UsedCount -= 1;
            var usage = await _context.PromotionUsages.FirstOrDefaultAsync(u => u.OrderId == request.OrderId && u.Status == "Used");
            if (usage != null)
            {
                usage.Status = "Reverted";
            }
            await _context.SaveChangesAsync();
        }

        return Ok(ApiResponse<bool>.Ok(true, "Hoàn lại lượt khuyến mãi thành công."));
    }
}
