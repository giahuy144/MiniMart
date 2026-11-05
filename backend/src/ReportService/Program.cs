using MiniMart.Shared.Extensions;
using MiniMart.Shared.Middlewares;
using MiniMart.Shared.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddCustomSwagger("MiniMart Report & Analytics Service");
builder.Services.AddCustomJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReportService v1"));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
