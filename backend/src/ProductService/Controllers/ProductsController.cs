using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Shared.Responses;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _context;

    public ProductsController(ProductDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductDto>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) || p.Barcode.Contains(s) || p.Sku.ToLower().Contains(s));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status.ToLower() == status.Trim().ToLower());
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Barcode = p.Barcode,
                Sku = p.Sku,
                Name = p.Name,
                Description = p.Description,
                Unit = p.Unit,
                CostPrice = p.CostPrice,
                SellingPrice = p.SellingPrice,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return Ok(PagedResponse<ProductDto>.Create(items, total, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductById(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound(ApiResponse<ProductDto>.Fail("Không tìm thấy sản phẩm."));
        }

        var dto = new ProductDto
        {
            Id = product.Id,
            Barcode = product.Barcode,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Unit = product.Unit,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            SupplierId = product.SupplierId,
            SupplierName = product.Supplier?.Name,
            Status = product.Status,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };

        return Ok(ApiResponse<ProductDto>.Ok(dto));
    }

    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductByBarcode(string barcode)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Barcode == barcode.Trim());

        if (product == null)
        {
            return NotFound(ApiResponse<ProductDto>.Fail($"Không tìm thấy sản phẩm với mã barcode: {barcode}"));
        }

        var dto = new ProductDto
        {
            Id = product.Id,
            Barcode = product.Barcode,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Unit = product.Unit,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            SupplierId = product.SupplierId,
            SupplierName = product.Supplier?.Name,
            Status = product.Status,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };

        return Ok(ApiResponse<ProductDto>.Ok(dto));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductDto dto)
    {
        if (await _context.Products.AnyAsync(p => p.Barcode == dto.Barcode.Trim()))
        {
            return BadRequest(ApiResponse<ProductDto>.Fail("Mã Barcode đã tồn tại trong hệ thống."));
        }

        var product = new Product
        {
            Barcode = dto.Barcode.Trim(),
            Sku = string.IsNullOrWhiteSpace(dto.Sku) ? $"SKU-{DateTime.UtcNow.Ticks % 1000000}" : dto.Sku.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description,
            Unit = dto.Unit,
            CostPrice = dto.CostPrice,
            SellingPrice = dto.SellingPrice,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, ApiResponse<ProductDto>.Ok(new ProductDto
        {
            Id = product.Id,
            Barcode = product.Barcode,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Unit = product.Unit,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            Status = product.Status,
            CreatedAt = product.CreatedAt
        }, "Tạo mới sản phẩm thành công."));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(ApiResponse<ProductDto>.Fail("Không tìm thấy sản phẩm cần cập nhật."));
        }

        if (!string.IsNullOrWhiteSpace(dto.Barcode) && dto.Barcode != product.Barcode)
        {
            if (await _context.Products.AnyAsync(p => p.Barcode == dto.Barcode && p.Id != id))
            {
                return BadRequest(ApiResponse<ProductDto>.Fail("Mã Barcode mới đã bị trùng với sản phẩm khác."));
            }
            product.Barcode = dto.Barcode.Trim();
        }

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Unit = dto.Unit;
        product.CostPrice = dto.CostPrice;
        product.SellingPrice = dto.SellingPrice;
        product.ImageUrl = dto.ImageUrl;
        product.CategoryId = dto.CategoryId;
        product.SupplierId = dto.SupplierId;
        product.Status = dto.Status;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<ProductDto>.Ok(null, "Cập nhật sản phẩm thành công."));
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangeStatus(int id, [FromBody] string status)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(ApiResponse<bool>.Fail("Không tìm thấy sản phẩm."));
        }

        product.Status = status;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, $"Đã cập nhật trạng thái sản phẩm thành: {status}"));
    }
}
