using Microsoft.AspNetCore.Mvc;
using MiniMart.Shared.Responses;
using ReportService.DTOs;

namespace ReportService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public ReportsController(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<RevenueReportDto>>> GetRevenueReport([FromQuery] string? period)
    {
        // Placeholder for revenue report logic
        var dto = new RevenueReportDto
        {
            TotalRevenue = 15000000,
            TotalOrders = 45,
            SuccessfulOrders = 42,
            CancelledOrders = 3,
            AverageOrderValue = 357142,
            DailyBreakdown = new List<DailyRevenueDto>
            {
                new() { Date = DateTime.UtcNow.ToString("yyyy-MM-dd"), Revenue = 2500000, OrderCount = 8 }
            }
        };

        return Ok(ApiResponse<RevenueReportDto>.Ok(dto));
    }

    [HttpGet("best-selling")]
    public async Task<ActionResult<ApiResponse<List<BestSellingProductDto>>>> GetBestSelling()
    {
        // Placeholder for best selling products
        var list = new List<BestSellingProductDto>
        {
            new() { ProductId = 1, Barcode = "8934673123456", ProductName = "Sữa tươi Vinamilk 180ml", QuantitySold = 120, TotalSales = 1200000 },
            new() { ProductId = 2, Barcode = "8934567890123", ProductName = "Mì Hảo Hảo tôm chua cay", QuantitySold = 95, TotalSales = 475000 }
        };

        return Ok(ApiResponse<List<BestSellingProductDto>>.Ok(list));
    }

    [HttpGet("inventory-status")]
    public async Task<ActionResult<ApiResponse<InventoryStatusReportDto>>> GetInventoryStatus()
    {
        // Placeholder for inventory status
        var dto = new InventoryStatusReportDto
        {
            TotalItems = 5,
            TotalStockQuantity = 255,
            LowStockItemsCount = 1,
            Items = new List<StockItemReportDto>()
        };

        return Ok(ApiResponse<InventoryStatusReportDto>.Ok(dto));
    }
}
