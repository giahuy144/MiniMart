using System.Security.Claims;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Shared.Constants;
using MiniMart.Shared.Responses;
using MiniMart.Shared.Security;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthController(AuthDbContext context, JwtSettings jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower());
        if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Tên đăng nhập hoặc mật khẩu không chính xác."));
        }

        if (!user.IsActive)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Tài khoản của bạn đã bị vô hiệu hóa."));
        }

        var token = JwtExtensions.GenerateJwtToken(_jwtSettings, user.Id.ToString(), user.Username, user.Email, user.Role);

        var response = new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
        };

        return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Đăng nhập thành công."));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterDto dto)
    {
        var existingUser = await _context.Users.AnyAsync(u => u.Username.ToLower() == dto.Username.ToLower());
        if (existingUser)
        {
            return BadRequest(ApiResponse<UserDto>.Fail("Tên đăng nhập đã tồn tại trong hệ thống."));
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var existingEmail = await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (existingEmail)
            {
                return BadRequest(ApiResponse<UserDto>.Fail("Email đã được sử dụng bởi tài khoản khác."));
            }
        }

        // Default role is Customer if not explicitly specified or unauthorized
        var role = string.IsNullOrWhiteSpace(dto.Role) ? RoleConstants.Customer : dto.Role;

        var user = new User
        {
            Username = dto.Username.Trim(),
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PhoneNumber = dto.PhoneNumber?.Trim() ?? string.Empty,
            PasswordHash = PasswordHasher.HashPassword(dto.Password),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return CreatedAtAction(nameof(GetMe), ApiResponse<UserDto>.Ok(result, "Đăng ký tài khoản thành công."));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetMe()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(ApiResponse<UserDto>.Fail("Không xác định được danh tính người dùng."));
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.Fail("Không tìm thấy thông tin người dùng."));
        }

        var result = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<UserDto>>.Ok(users));
    }
}
