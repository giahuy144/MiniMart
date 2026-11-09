using CustomerService.Data;
using Microsoft.EntityFrameworkCore;
using MiniMart.Shared.Extensions;
using MiniMart.Shared.Middlewares;
using MiniMart.Shared.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCustomSwagger("MiniMart Customer Service");

var databaseProvider = builder.Configuration["Database:Provider"] ?? "MySql";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    if (databaseProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
    {
        options.UseInMemoryDatabase("MiniMart_CustomerDb");
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

builder.Services.AddCustomJwtAuthentication(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Could not initialize CustomerService database using {DatabaseProvider}.", databaseProvider);
        throw;
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CustomerService v1"));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
