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
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
        if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Email hoặc mật khẩu không chính xác."));
        }

        if (!user.IsActive)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Tài khoản của bạn đã bị vô hiệu hóa."));
        }

        var token = JwtExtensions.GenerateJwtToken(
            _jwtSettings,
            user.Id.ToString(),
            user.Username,
            user.Email,
            user.Role);

        return Ok(ApiResponse<AuthResponseDto>.Ok(
            CreateAuthResponse(user, token),
            "Đăng nhập thành công."));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterDto dto)
    {
        var username = dto.Username.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
        {
            return BadRequest(ApiResponse<UserDto>.Fail("Tên đăng nhập đã tồn tại trong hệ thống."));
        }

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
        {
            return BadRequest(ApiResponse<UserDto>.Fail("Email đã được sử dụng bởi tài khoản khác."));
        }

        var user = new User
        {
            Username = username,
            FullName = dto.FullName.Trim(),
            Email = email,
            PhoneNumber = dto.PhoneNumber?.Trim() ?? string.Empty,
            PasswordHash = PasswordHasher.HashPassword(dto.Password),
            Role = RoleConstants.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMe),
            ApiResponse<UserDto>.Ok(ToUserDto(user), "Đăng ký tài khoản thành công."));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetMe()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<UserDto>.Fail("Không xác định được danh tính người dùng."));
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.Fail("Không tìm thấy thông tin người dùng."));
        }

        return Ok(ApiResponse<UserDto>.Ok(ToUserDto(user)));
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<bool>.Fail("Không xác định được danh tính người dùng."));
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse<bool>.Fail("Không tìm thấy thông tin người dùng."));
        }

        if (!PasswordHasher.VerifyPassword(dto.OldPassword, user.PasswordHash))
        {
            return BadRequest(ApiResponse<bool>.Fail("Mật khẩu cũ không chính xác."));
        }

        user.PasswordHash = PasswordHasher.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Đổi mật khẩu thành công."));
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(ApiResponse<UserDto>.Fail("Không xác định được danh tính người dùng."));
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.Fail("Không tìm thấy thông tin người dùng."));
        }

        user.FullName = dto.FullName.Trim();
        user.PhoneNumber = dto.PhoneNumber.Trim();
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<UserDto>.Ok(ToUserDto(user), "Cập nhật thông tin cá nhân thành công."));
    }

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
    {
        var users = await _context.Users
            .OrderByDescending(user => user.CreatedAt)
            .Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<UserDto>>.Ok(users));
    }

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpPatch("users/{id:guid}/toggle-status")]
    public async Task<ActionResult<ApiResponse<UserDto>>> ToggleUserStatus(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.Fail("Không tìm thấy tài khoản."));
        }

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();

        var message = user.IsActive ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.";
        return Ok(ApiResponse<UserDto>.Ok(ToUserDto(user), message));
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out userId);
    }

    private AuthResponseDto CreateAuthResponse(User user, string token) => new()
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

    private static UserDto ToUserDto(User user) => new()
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
}
