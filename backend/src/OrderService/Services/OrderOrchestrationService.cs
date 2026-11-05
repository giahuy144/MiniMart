using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using MiniMart.Shared.Responses;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Services;

public interface IOrderOrchestrationService
{
    Task<ApiResponse<OrderDto>> ProcessOrderCheckoutAsync(CreateOrderRequestDto request, string username);
    Task<ApiResponse<bool>> CancelOrderAsync(Guid orderId, string reason);
}

public class OrderOrchestrationService : IOrderOrchestrationService
{
    private readonly OrderDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<OrderOrchestrationService> _logger;

    public OrderOrchestrationService(
        OrderDbContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<OrderOrchestrationService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    private string ProductServiceUrl => _config["Services:ProductServiceUrl"] ?? "http://localhost:5002";
    private string InventoryServiceUrl => _config["Services:InventoryServiceUrl"] ?? "http://localhost:5003";
    private string PromotionServiceUrl => _config["Services:PromotionServiceUrl"] ?? "http://localhost:5004";
    private string CustomerServiceUrl => _config["Services:CustomerServiceUrl"] ?? "http://localhost:5006";

    public async Task<ApiResponse<OrderDto>> ProcessOrderCheckoutAsync(CreateOrderRequestDto request, string username)
    {
        var client = _httpClientFactory.CreateClient();
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName ?? "Khách vãng lai",
            CustomerPhone = request.CustomerPhone ?? string.Empty,
            CreatedBy = username,
            Status = "Pending",
            PaymentStatus = "Unpaid",
            PromotionCode = request.PromotionCode?.Trim() ?? string.Empty,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        // BƯỚC 1: Gọi Product Service để lấy thông tin snapshot và giá bán
        decimal subTotal = 0;
        foreach (var item in request.Items)
        {
            try
            {
                var prodRes = await client.GetFromJsonAsync<ApiResponse<ProductTempDto>>($"{ProductServiceUrl}/api/products/{item.ProductId}");
                if (prodRes == null || !prodRes.Success || prodRes.Data == null)
                {
                    return ApiResponse<OrderDto>.Fail($"Không thể tìm thấy thông tin sản phẩm #{item.ProductId} từ Product Service.");
                }

                if (prodRes.Data.Status != "Active")
                {
                    return ApiResponse<OrderDto>.Fail($"Sản phẩm '{prodRes.Data.Name}' đã ngừng kinh doanh.");
                }

                var detail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = prodRes.Data.Id,
                    Barcode = prodRes.Data.Barcode,
                    ProductName = prodRes.Data.Name,
                    UnitPrice = prodRes.Data.SellingPrice,
                    Quantity = item.Quantity
                };

                subTotal += detail.TotalPrice;
                order.Details.Add(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi ProductService");
                return ApiResponse<OrderDto>.Fail("Không thể kết nối đến Product Service để xác thực sản phẩm.");
            }
        }

        order.SubTotal = subTotal;
        decimal discountAmount = 0;

        // BƯỚC 2: Gọi Promotion Service để tính giảm giá (nếu có mã)
        if (!string.IsNullOrWhiteSpace(order.PromotionCode))
        {
            try
            {
                var promoReq = new
                {
                    PromoCode = order.PromotionCode,
                    OrderTotal = subTotal,
                    CustomerId = request.CustomerId?.ToString()
                };

                var promoRes = await client.PostAsJsonAsync($"{PromotionServiceUrl}/api/promotions/calculate", promoReq);
                if (promoRes.IsSuccessStatusCode)
                {
                    var promoData = await promoRes.Content.ReadFromJsonAsync<ApiResponse<PromoCalculateResultTempDto>>();
                    if (promoData != null && promoData.Success && promoData.Data != null && promoData.Data.IsValid)
                    {
                        discountAmount = promoData.Data.DiscountAmount;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể kết nối Promotion Service, tiếp tục với 0 giảm giá");
            }
        }

        order.DiscountAmount = discountAmount;
        order.TotalAmount = Math.Max(0, subTotal - discountAmount);

        // BƯỚC 3: Gọi Inventory Service để kiểm tra và trừ kho
        var deductReq = new
        {
            OrderId = order.OrderNumber,
            Items = order.Details.Select(d => new { d.ProductId, d.Quantity }).ToList(),
            PerformedBy = username
        };

        bool inventoryDeducted = false;
        try
        {
            var invRes = await client.PostAsJsonAsync($"{InventoryServiceUrl}/api/inventory/deduct", deductReq);
            if (!invRes.IsSuccessStatusCode)
            {
                var invError = await invRes.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                order.Status = "Failed";
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return ApiResponse<OrderDto>.Fail(invError?.Message ?? "Không đủ hàng trong kho để tạo đơn.");
            }
            inventoryDeducted = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi kết nối Inventory Service khi trừ kho");
            order.Status = "Failed";
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return ApiResponse<OrderDto>.Fail("Lỗi kết nối đến Inventory Service khi kiểm tra tồn kho.");
        }

        // BƯỚC 4: Hoàn tất đơn hàng và ghi nhận thanh toán
        try
        {
            order.Status = "Confirmed";
            order.PaymentStatus = "Paid";

            order.Payments.Add(new Payment
            {
                OrderId = order.Id,
                PaymentMethod = request.PaymentMethod,
                Amount = order.TotalAmount,
                TransactionReference = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Status = "Success",
                PaymentTime = DateTime.UtcNow
            });

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Ghi nhận lượt sử dụng khuyến mãi (nếu có áp dụng)
            if (!string.IsNullOrWhiteSpace(order.PromotionCode) && discountAmount > 0)
            {
                _ = client.PostAsJsonAsync($"{PromotionServiceUrl}/api/promotions/record-usage", new
                {
                    PromoCode = order.PromotionCode,
                    OrderId = order.OrderNumber,
                    CustomerId = request.CustomerId?.ToString(),
                    DiscountAmount = discountAmount
                });
            }

            // Gọi Customer Service cộng điểm tích lũy
            if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
            {
                _ = client.PostAsJsonAsync($"{CustomerServiceUrl}/api/customers/{request.CustomerId.Value}/points/add", new
                {
                    OrderId = order.OrderNumber,
                    OrderTotal = order.TotalAmount
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra trong quá trình hoàn tất đơn hàng, tiến hành Rollback Kho");

            // BƯỚC 5: ROLLBACK nếu có lỗi sau khi trừ kho
            if (inventoryDeducted)
            {
                var rollbackReq = new
                {
                    OrderId = order.OrderNumber,
                    Items = order.Details.Select(d => new { d.ProductId, d.Quantity }).ToList(),
                    Reason = "Lỗi hệ thống khi lưu hóa đơn"
                };
                _ = client.PostAsJsonAsync($"{InventoryServiceUrl}/api/inventory/rollback", rollbackReq);
            }

            order.Status = "Failed";
            await _context.SaveChangesAsync();
            return ApiResponse<OrderDto>.Fail("Đã xảy ra lỗi trong quá trình xử lý đơn hàng. Tồn kho đã được hoàn tác an toàn.");
        }

        var resultDto = MapToDto(order);
        return ApiResponse<OrderDto>.Ok(resultDto, "Tạo đơn hàng và thanh toán thành công!");
    }

    public async Task<ApiResponse<bool>> CancelOrderAsync(Guid orderId, string reason)
    {
        var order = await _context.Orders.Include(o => o.Details).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            return ApiResponse<bool>.Fail("Không tìm thấy đơn hàng.");
        }

        if (order.Status == "Cancelled")
        {
            return ApiResponse<bool>.Fail("Đơn hàng này đã được hủy trước đó.");
        }

        var client = _httpClientFactory.CreateClient();

        // 1. Hoàn lại kho
        var rollbackReq = new
        {
            OrderId = order.OrderNumber,
            Items = order.Details.Select(d => new { d.ProductId, d.Quantity }).ToList(),
            Reason = reason
        };
        _ = client.PostAsJsonAsync($"{InventoryServiceUrl}/api/inventory/rollback", rollbackReq);

        // 2. Hoàn lại lượt khuyến mãi
        if (!string.IsNullOrWhiteSpace(order.PromotionCode))
        {
            _ = client.PostAsJsonAsync($"{PromotionServiceUrl}/api/promotions/rollback-usage", new
            {
                PromoCode = order.PromotionCode,
                OrderId = order.OrderNumber
            });
        }

        // 3. Hoàn lại điểm tích lũy của khách hàng
        if (order.CustomerId.HasValue)
        {
            _ = client.PostAsJsonAsync($"{CustomerServiceUrl}/api/customers/{order.CustomerId.Value}/points/rollback", new
            {
                OrderId = order.OrderNumber,
                Reason = reason
            });
        }

        order.Status = "Cancelled";
        order.Notes += $" | Đã hủy đơn: {reason}";
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Hủy đơn hàng và hoàn trả tài nguyên thành công.");
    }

    private static OrderDto MapToDto(Order o) => new()
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
            TotalPrice = d.TotalPrice
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
}

// Temporary DTOs for deserializing responses from other services
public class ProductTempDto
{
    public int Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PromoCalculateResultTempDto
{
    public bool IsValid { get; set; }
    public decimal DiscountAmount { get; set; }
}
