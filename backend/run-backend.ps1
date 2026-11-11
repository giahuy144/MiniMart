param(
    [ValidateSet("MySql", "InMemory")]
    [string]$DatabaseProvider = "MySql",
    [int]$StartupTimeoutSeconds = 30
)

$ErrorActionPreference = "Stop"
$services = @(
    @{ Name = "API Gateway"; Path = "src/ApiGateway"; Port = 5000 },
    @{ Name = "Auth Service"; Path = "src/AuthService"; Port = 5001 },
    @{ Name = "Product Service"; Path = "src/ProductService"; Port = 5002 },
    @{ Name = "Inventory Service"; Path = "src/InventoryService"; Port = 5003 },
    @{ Name = "Promotion Service"; Path = "src/PromotionService"; Port = 5004 },
    @{ Name = "Order Service"; Path = "src/OrderService"; Port = 5005 },
    @{ Name = "Customer Service"; Path = "src/CustomerService"; Port = 5006 },
    @{ Name = "Report Service"; Path = "src/ReportService"; Port = 5007 }
)

function Test-LocalPort([int]$Port) {
    $client = New-Object System.Net.Sockets.TcpClient
    try {
        $connect = $client.BeginConnect("127.0.0.1", $Port, $null, $null)
        return $connect.AsyncWaitHandle.WaitOne(250) -and $client.Connected
    } catch {
        return $false
    } finally {
        $client.Close()
    }
}

if ($DatabaseProvider -eq "MySql") {
    $mysql = New-Object System.Net.Sockets.TcpClient
    try {
        $connect = $mysql.BeginConnect("127.0.0.1", 3306, $null, $null)
        $mysqlReady = $connect.AsyncWaitHandle.WaitOne(1200) -and $mysql.Connected
        if ($mysqlReady) { $mysql.EndConnect($connect) }
    } catch {
        $mysqlReady = $false
    } finally {
        $mysql.Close()
    }

    if (-not $mysqlReady) {
        Write-Error "MySQL khong mo tai 127.0.0.1:3306. Hay bat MySQL hoac chay: .\run-backend.ps1 -DatabaseProvider InMemory"
        exit 1
    }
}

$logDirectory = Join-Path $PSScriptRoot "logs"
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
$previousProvider = $env:Database__Provider

try {
    $env:Database__Provider = $DatabaseProvider
    foreach ($service in $services) {
        if (Test-LocalPort $service.Port) {
            Write-Host "Port $($service.Port) da co service lang nghe; bo qua $($service.Name)." -ForegroundColor Yellow
            continue
        }

        $workingDirectory = Join-Path $PSScriptRoot $service.Path
        $stdout = Join-Path $logDirectory "$($service.Port)-stdout.log"
        $stderr = Join-Path $logDirectory "$($service.Port)-stderr.log"
        $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory $workingDirectory -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru
        Write-Host "Da khoi chay $($service.Name) (port $($service.Port), PID $($process.Id))."
    }
}
finally {
    if ($null -eq $previousProvider) {
        Remove-Item Env:Database__Provider -ErrorAction SilentlyContinue
    } else {
        $env:Database__Provider = $previousProvider
    }
}

$deadline = (Get-Date).AddSeconds($StartupTimeoutSeconds)
do {
    $pending = @($services | Where-Object { -not (Test-LocalPort $_.Port) })
    if ($pending.Count -eq 0) { break }
    Start-Sleep -Milliseconds 500
} while ((Get-Date) -lt $deadline)

$failed = @($services | Where-Object { -not (Test-LocalPort $_.Port) })
if ($failed.Count -eq 0) {
    Write-Host "Tat ca 8 service dang lang nghe." -ForegroundColor Green
} else {
    Write-Host "Chua san sang: $(( $failed | ForEach-Object { "$($_.Name) : $($_.Port)" }) -join ', ')" -ForegroundColor Red
    Write-Host "Xem log tai: $logDirectory" -ForegroundColor Yellow
    exit 1
}

if ($DatabaseProvider -eq "InMemory") {
    Write-Host "Dang dung InMemory: du lieu seed dung de test va se mat khi dong service." -ForegroundColor Yellow
}
Write-Host "Gateway: http://localhost:5000"
Write-Host "Logs: $logDirectory"

