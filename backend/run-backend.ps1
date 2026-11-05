# Script khởi chạy toàn bộ 8 services của hệ thống MiniMart SOA

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   KHOI DONG HE THONG MINIMART BACKEND (SOA ARCHITECTURE) " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$services = @(
    @{ Name = "MiniMart API Gateway"; Path = "src/ApiGateway"; Port = 5000 },
    @{ Name = "Auth / User Service"; Path = "src/AuthService"; Port = 5001 },
    @{ Name = "Product Service"; Path = "src/ProductService"; Port = 5002 },
    @{ Name = "Inventory Service"; Path = "src/InventoryService"; Port = 5003 },
    @{ Name = "Promotion Service"; Path = "src/PromotionService"; Port = 5004 },
    @{ Name = "Order / Sales Service"; Path = "src/OrderService"; Port = 5005 },
    @{ Name = "Customer Service"; Path = "src/CustomerService"; Port = 5006 },
    @{ Name = "Report Service"; Path = "src/ReportService"; Port = 5007 }
)

foreach ($s in $services) {
    Write-Host "Dang khoi chay $($s.Name) tai http://localhost:$($s.Port)..." -ForegroundColor Green
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd $(Join-Path $PSScriptRoot $s.Path); dotnet run"
}

Write-Host "`nToan bo 8 services da duoc mo trong cac cua so rieng biet!" -ForegroundColor Yellow
Write-Host "API Gateway chay tai: http://localhost:5000" -ForegroundColor Cyan
Write-Host "Swagger docs cua tung service:" -ForegroundColor Cyan
Write-Host " - AuthService:       http://localhost:5001/swagger"
Write-Host " - ProductService:    http://localhost:5002/swagger"
Write-Host " - InventoryService:  http://localhost:5003/swagger"
Write-Host " - PromotionService:  http://localhost:5004/swagger"
Write-Host " - OrderService:      http://localhost:5005/swagger"
Write-Host " - CustomerService:   http://localhost:5006/swagger"
Write-Host " - ReportService:     http://localhost:5007/swagger"
