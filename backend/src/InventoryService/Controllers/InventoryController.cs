using InventoryService.Data;
using InventoryService.DTOs;
using InventoryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Shared.Responses;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryDbContext _context;

    public InventoryController(InventoryDbContext context)
    {
        _context = context;
    }

    [HttpGet("stock")]
    public async Task<ActionResult<ApiResponse<List<StockDto>>>> GetStock(
        [FromQuery] bool? lowStockOnly,
        [FromQuery] string? search)
    {
        var query = _context.Stocks.AsQueryable();

        if (lowStockOnly == true)
        {
            query = query.Where(s => s.Quantity <= s.MinStockLevel);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(x => x.ProductName.ToLower().Contains(s) || x.Barcode.Contains(s));
        }

        var list = await query
            .OrderBy(s => s.Quantity)
            .Select(s => new StockDto
            {
                Id = s.Id,
                ProductId = s.ProductId,
                ProductName = s.ProductName,
                Barcode = s.Barcode,
                Quantity = s.Quantity,
                MinStockLevel = s.MinStockLevel,
                IsLowStock = s.Quantity <= s.MinStockLevel,
                LastUpdated = s.LastUpdated
            })
            .ToListAsync();

        return Ok(ApiResponse<List<StockDto>>.Ok(list));
    }

    [HttpGet("stock/{productId}")]
    public async Task<ActionResult<ApiResponse<StockDto>>> GetStockByProductId(int productId)
    {
        var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProductId == productId);
        if (stock == null)
        {
            return NotFound(ApiResponse<StockDto>.Fail("Không tìm thấy thông tin tồn kho cho sản phẩm này."));
        }

        var dto = new StockDto
        {
            Id = stock.Id,
            ProductId = stock.ProductId,
            ProductName = stock.ProductName,
            Barcode = stock.Barcode,
            Quantity = stock.Quantity,
            MinStockLevel = stock.MinStockLevel,
            IsLowStock = stock.Quantity <= stock.MinStockLevel,
            LastUpdated = stock.LastUpdated
        };

        return Ok(ApiResponse<StockDto>.Ok(dto));
    }

    [HttpPost("deduct")]
    public async Task<ActionResult<ApiResponse<bool>>> DeductStock([FromBody] DeductStockRequestDto request)
    {
        if (request.Items == null || !request.Items.Any())
        {
            return BadRequest(ApiResponse<bool>.Fail("Danh sách sản phẩm trừ kho rỗng."));
        }

        // 1. Verify all products have sufficient stock
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var stockRecords = await _context.Stocks
            .Where(s => productIds.Contains(s.ProductId))
            .ToDictionaryAsync(s => s.ProductId);

        foreach (var item in request.Items)
        {
            if (!stockRecords.TryGetValue(item.ProductId, out var stock))
            {
                return BadRequest(ApiResponse<bool>.Fail($"Sản phẩm mã #{item.ProductId} không có dữ liệu tồn kho."));
            }

            if (stock.Quantity < item.Quantity)
            {
                return BadRequest(ApiResponse<bool>.Fail($"Sản phẩm '{stock.ProductName}' chỉ còn {stock.Quantity} trong kho, không đủ số lượng yêu cầu ({item.Quantity})."));
            }
        }

        // 2. Perform stock reduction and record transactions
        foreach (var item in request.Items)
        {
            var stock = stockRecords[item.ProductId];
            var before = stock.Quantity;
            stock.Quantity -= item.Quantity;
            stock.LastUpdated = DateTime.UtcNow;

            _context.StockTransactions.Add(new StockTransaction
            {
                ProductId = item.ProductId,
                ProductName = stock.ProductName,
                TransactionType = "Sale",
                QuantityChange = -item.Quantity,
                QuantityBefore = before,
                QuantityAfter = stock.Quantity,
                ReferenceId = request.OrderId,
                Note = $"Trừ kho cho đơn hàng #{request.OrderId}",
                CreatedBy = request.PerformedBy,
                CreatedDate = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Trừ kho thành công."));
    }

    [HttpPost("rollback")]
    public async Task<ActionResult<ApiResponse<bool>>> RollbackStock([FromBody] RollbackStockRequestDto request)
    {
        if (request.Items == null || !request.Items.Any())
        {
            return BadRequest(ApiResponse<bool>.Fail("Danh sách sản phẩm hoàn kho rỗng."));
        }

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var stockRecords = await _context.Stocks
            .Where(s => productIds.Contains(s.ProductId))
            .ToDictionaryAsync(s => s.ProductId);

        foreach (var item in request.Items)
        {
            if (stockRecords.TryGetValue(item.ProductId, out var stock))
            {
                var before = stock.Quantity;
                stock.Quantity += item.Quantity;
                stock.LastUpdated = DateTime.UtcNow;

                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductId = item.ProductId,
                    ProductName = stock.ProductName,
                    TransactionType = "Rollback",
                    QuantityChange = item.Quantity,
                    QuantityBefore = before,
                    QuantityAfter = stock.Quantity,
                    ReferenceId = request.OrderId,
                    Note = $"Hoàn kho do hủy/lỗi đơn hàng #{request.OrderId}. Lý do: {request.Reason}",
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Hoàn kho thành công."));
    }

    [HttpPost("import-receipts")]
    public async Task<ActionResult<ApiResponse<ImportReceiptDto>>> CreateImportReceipt([FromBody] CreateImportReceiptDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
        {
            return BadRequest(ApiResponse<ImportReceiptDto>.Fail("Phiếu nhập phải có ít nhất 1 sản phẩm."));
        }

        var receiptNumber = $"PN-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var receipt = new ImportReceipt
        {
            ReceiptNumber = receiptNumber,
            SupplierId = dto.SupplierId,
            SupplierName = dto.SupplierName,
            CreatedBy = dto.CreatedBy,
            CreatedDate = DateTime.UtcNow,
            TotalAmount = dto.Items.Sum(i => i.Quantity * i.ImportPrice),
            Status = "Confirmed",
            Note = dto.Note
        };

        foreach (var item in dto.Items)
        {
            receipt.Details.Add(new ImportReceiptDetail
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                ImportPrice = item.ImportPrice
            });

            // Update or create Stock
            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProductId == item.ProductId);
            var before = 0;
            if (stock == null)
            {
                stock = new Stock
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Barcode = item.Barcode,
                    Quantity = item.Quantity,
                    MinStockLevel = 10,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Stocks.Add(stock);
            }
            else
            {
                before = stock.Quantity;
                stock.Quantity += item.Quantity;
                stock.LastUpdated = DateTime.UtcNow;
            }

            _context.StockTransactions.Add(new StockTransaction
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                TransactionType = "Import",
                QuantityChange = item.Quantity,
                QuantityBefore = before,
                QuantityAfter = before + item.Quantity,
                ReferenceId = receiptNumber,
                Note = $"Nhập hàng từ phiếu nhập #{receiptNumber}",
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.UtcNow
            });
        }

        _context.ImportReceipts.Add(receipt);
        await _context.SaveChangesAsync();

        var resultDto = new ImportReceiptDto
        {
            Id = receipt.Id,
            ReceiptNumber = receipt.ReceiptNumber,
            SupplierId = receipt.SupplierId,
            SupplierName = receipt.SupplierName,
            CreatedBy = receipt.CreatedBy,
            CreatedDate = receipt.CreatedDate,
            TotalAmount = receipt.TotalAmount,
            Status = receipt.Status,
            Note = receipt.Note
        };

        return Ok(ApiResponse<ImportReceiptDto>.Ok(resultDto, "Tạo phiếu nhập và cập nhật tồn kho thành công."));
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<ApiResponse<List<StockTransaction>>>> GetTransactions([FromQuery] int? productId)
    {
        var query = _context.StockTransactions.AsQueryable();
        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(t => t.ProductId == productId.Value);
        }

        var list = await query.OrderByDescending(t => t.CreatedDate).Take(100).ToListAsync();
        return Ok(ApiResponse<List<StockTransaction>>.Ok(list));
    }
}
