using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using MiniMart.Shared.Constants;
using MiniMart.Shared.Extensions;
using MiniMart.Shared.Middlewares;
using MiniMart.Shared.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCustomSwagger("MiniMart Auth & User Service");

// Database configuration with SQL Server (or InMemory fallback)
var databaseProvider = builder.Configuration["Database:Provider"] ?? "MySql";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    if (databaseProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
    {
        options.UseInMemoryDatabase("MiniMart_AuthDb");
    }
    else if (databaseProvider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required for MySQL.");
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
    else
    {
        throw new InvalidOperationException($"Unsupported Database:Provider '{databaseProvider}'. Use MySql or InMemory.");
    }
});

// Configure JWT
builder.Services.AddCustomJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Auto create database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    try
    {
        context.Database.EnsureCreated();
        var demoAccounts = new[]
        {
            new User { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Username = "admin", FullName = "Quản Trị Viên", Email = "admin@gmail.com", PhoneNumber = "0900000001", Role = RoleConstants.Admin },
            new User { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Username = "cashier", FullName = "Nhân Viên Thu Ngân", Email = "cashier@gmail.com", PhoneNumber = "0900000002", Role = RoleConstants.Cashier },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Username = "stockkeeper", FullName = "Thủ Kho Hàng", Email = "stock@gmail.com", PhoneNumber = "0900000003", Role = RoleConstants.Stockkeeper },
            new User { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Username = "customer", FullName = "Nguyễn Văn Khách", Email = "customer@gmail.com", PhoneNumber = "0900000004", Role = RoleConstants.Customer },
        };
        foreach (var demo in demoAccounts)
        {
            var account = context.Users.FirstOrDefault(user => user.Id == demo.Id);
            if (account is null)
            {
                demo.PasswordHash = PasswordHasher.HashPassword("123456");
                demo.IsActive = true;
                demo.CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                context.Users.Add(demo);
                continue;
            }

            account.Username = demo.Username;
            account.FullName = demo.FullName;
            account.Email = demo.Email;
            account.PhoneNumber = demo.PhoneNumber;
            account.Role = demo.Role;
            account.PasswordHash = PasswordHasher.HashPassword("123456");
            account.IsActive = true;
        }
        context.SaveChanges();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Could not initialize AuthService database using {DatabaseProvider}.", databaseProvider);
        throw;
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthService v1"));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
