using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Shared.Responses;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly IOrderOrchestrationService _orchestrationService;

    public OrdersController(OrderDbContext context, IOrderOrchestrationService orchestrationService)
    {
        _context = context;
        _orchestrationService = orchestrationService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<OrderDto>>> GetOrders(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int? customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Orders
            .Include(o => o.Details)
            .Include(o => o.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.Status.ToLower() == status.Trim().ToLower());
        }

        if (customerId.HasValue && customerId.Value > 0)
        {
            query = query.Where(o => o.CustomerId == customerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(o => o.OrderNumber.ToLower().Contains(s) || o.CustomerName.ToLower().Contains(s) || o.CustomerPhone.Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                CreatedBy = o.CreatedBy,
                SubTotal = o.SubTotal,
                DiscountAmount = o.DiscountAmount,
                TotalAmount = o.TotalAmount,
                PromotionCode = o.PromotionCode,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                Notes = o.Notes,
                CreatedAt = o.CreatedAt,
                Details = o.Details.Select(d => new OrderDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    Barcode = d.Barcode,
                    ProductName = d.ProductName,
                    UnitPrice = d.UnitPrice,
                    Quantity = d.Quantity,
                    TotalPrice = d.UnitPrice * d.Quantity
                }).ToList(),
                Payments = o.Payments.Select(p => new PaymentDto
                {
                    Id = p.Id,
                    PaymentMethod = p.PaymentMethod,
                    Amount = p.Amount,
                    TransactionReference = p.TransactionReference,
                    Status = p.Status,
                    PaymentTime = p.PaymentTime
                }).ToList()
            })
            .ToListAsync();

        return Ok(PagedResponse<OrderDto>.Create(items, total, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderById(Guid id)
    {
        var o = await _context.Orders
            .Include(x => x.Details)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (o == null)
        {
            return NotFound(ApiResponse<OrderDto>.Fail("Không tìm thấy đơn hàng."));
        }

        var dto = new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerId = o.CustomerId,
            CustomerName = o.CustomerName,
            CustomerPhone = o.CustomerPhone,
            CreatedBy = o.CreatedBy,
            SubTotal = o.SubTotal,
            DiscountAmount = o.DiscountAmount,
            TotalAmount = o.TotalAmount,
            PromotionCode = o.PromotionCode,
            Status = o.Status,
            PaymentStatus = o.PaymentStatus,
            Notes = o.Notes,
            CreatedAt = o.CreatedAt,
            Details = o.Details.Select(d => new OrderDetailDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                Barcode = d.Barcode,
                ProductName = d.ProductName,
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity,
                TotalPrice = d.UnitPrice * d.Quantity
            }).ToList(),
            Payments = o.Payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                TransactionReference = p.TransactionReference,
                Status = p.Status,
                PaymentTime = p.PaymentTime
            }).ToList()
        };

        return Ok(ApiResponse<OrderDto>.Ok(dto));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderRequestDto request)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? "Thu Ngân";
        var result = await _orchestrationService.ProcessOrderCheckoutAsync(request, username);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<bool>>> CancelOrder(Guid id, [FromBody] CancelOrderRequestDto request)
    {
        var result = await _orchestrationService.CancelOrderAsync(id, request.Reason);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
