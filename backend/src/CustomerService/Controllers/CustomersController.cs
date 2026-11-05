using CustomerService.Data;
using CustomerService.DTOs;
using CustomerService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Shared.Responses;

namespace CustomerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly CustomerDbContext _context;

    public CustomersController(CustomerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CustomerDto>>>> GetCustomers([FromQuery] string? search)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c => c.FullName.ToLower().Contains(s) || c.PhoneNumber.Contains(s) || c.Email.ToLower().Contains(s));
        }

        var list = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                UserId = c.UserId,
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                Address = c.Address,
                Points = c.Points,
                TotalSpent = c.TotalSpent,
                Tier = c.Tier,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<CustomerDto>>.Ok(list));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerById(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound(ApiResponse<CustomerDto>.Fail("Không tìm thấy khách hàng."));
        }

        var dto = new CustomerDto
        {
            Id = customer.Id,
            UserId = customer.UserId,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Address = customer.Address,
            Points = customer.Points,
            TotalSpent = customer.TotalSpent,
            Tier = customer.Tier,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };

        return Ok(ApiResponse<CustomerDto>.Ok(dto));
    }

    [HttpGet("by-phone/{phone}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetByPhone(string phone)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phone.Trim());
        if (customer == null)
        {
            return NotFound(ApiResponse<CustomerDto>.Fail($"Không tìm thấy khách hàng với số điện thoại: {phone}"));
        }

        var dto = new CustomerDto
        {
            Id = customer.Id,
            UserId = customer.UserId,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Address = customer.Address,
            Points = customer.Points,
            TotalSpent = customer.TotalSpent,
            Tier = customer.Tier,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };

        return Ok(ApiResponse<CustomerDto>.Ok(dto));
    }

    [HttpGet("by-user/{userId}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetByUserId(Guid userId)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer == null)
        {
            return NotFound(ApiResponse<CustomerDto>.Fail("Không tìm thấy thông tin khách hàng cho tài khoản này."));
        }

        var dto = new CustomerDto
        {
            Id = customer.Id,
            UserId = customer.UserId,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Address = customer.Address,
            Points = customer.Points,
            TotalSpent = customer.TotalSpent,
            Tier = customer.Tier,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };

        return Ok(ApiResponse<CustomerDto>.Ok(dto));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        if (await _context.Customers.AnyAsync(c => c.PhoneNumber == dto.PhoneNumber.Trim()))
        {
            return BadRequest(ApiResponse<CustomerDto>.Fail("Số điện thoại này đã được đăng ký khách hàng."));
        }

        var customer = new Customer
        {
            UserId = dto.UserId,
            FullName = dto.FullName.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Email = dto.Email.Trim(),
            Address = dto.Address.Trim(),
            Points = 0,
            TotalSpent = 0,
            Tier = "Thường",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var result = new CustomerDto
        {
            Id = customer.Id,
            UserId = customer.UserId,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Address = customer.Address,
            Points = customer.Points,
            TotalSpent = customer.TotalSpent,
            Tier = customer.Tier,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };

        return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, ApiResponse<CustomerDto>.Ok(result, "Tạo khách hàng thành công."));
    }

    [HttpPost("{id}/points/add")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> AddPoints(int id, [FromBody] AddPointsRequestDto request)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound(ApiResponse<CustomerDto>.Fail("Không tìm thấy khách hàng."));
        }

        // Quy tắc: 1 điểm cho mỗi 10,000 VND
        int pointsEarned = request.CustomPoints ?? (int)(request.OrderTotal / 10000m);
        customer.Points += pointsEarned;
        customer.TotalSpent += request.OrderTotal;
        customer.UpdateTier();

        _context.PointHistories.Add(new PointHistory
        {
            CustomerId = customer.Id,
            PointsChanged = pointsEarned,
            PointsBalanceAfter = customer.Points,
            TransactionType = "Earn",
            ReferenceId = request.OrderId,
            Reason = $"Tích điểm đơn hàng #{request.OrderId}",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var result = new CustomerDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Points = customer.Points,
            TotalSpent = customer.TotalSpent,
            Tier = customer.Tier
        };

        return Ok(ApiResponse<CustomerDto>.Ok(result, $"Đã tích thêm {pointsEarned} điểm thành công."));
    }

    [HttpPost("{id}/points/rollback")]
    public async Task<ActionResult<ApiResponse<bool>>> RollbackPoints(int id, [FromBody] RollbackPointsRequestDto request)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound(ApiResponse<bool>.Fail("Không tìm thấy khách hàng."));
        }

        var earnHistory = await _context.PointHistories
            .FirstOrDefaultAsync(h => h.CustomerId == id && h.ReferenceId == request.OrderId && h.TransactionType == "Earn");

        if (earnHistory != null && earnHistory.PointsChanged > 0)
        {
            customer.Points = Math.Max(0, customer.Points - earnHistory.PointsChanged);

            _context.PointHistories.Add(new PointHistory
            {
                CustomerId = customer.Id,
                PointsChanged = -earnHistory.PointsChanged,
                PointsBalanceAfter = customer.Points,
                TransactionType = "Rollback",
                ReferenceId = request.OrderId,
                Reason = $"Hoàn điểm do đơn hàng #{request.OrderId} bị hủy",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        return Ok(ApiResponse<bool>.Ok(true, "Hoàn điểm thành công."));
    }

    [HttpGet("{id}/points/history")]
    public async Task<ActionResult<ApiResponse<List<PointHistory>>>> GetPointsHistory(int id)
    {
        var history = await _context.PointHistories
            .Where(h => h.CustomerId == id)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<PointHistory>>.Ok(history));
    }
}
