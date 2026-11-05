# Hệ Thống Quản Lý Siêu Thị Mini (MiniMart)
**Học phần: Kiến trúc Hướng Dịch vụ (SOA) - Trường Đại học Quy Nhơn**

---

## 1. Cấu trúc Solution Backend (.NET 9)

```text
backend/
├── MiniMart.sln                     # Visual Studio Solution chứa toàn bộ 9 projects
├── run-backend.ps1                  # Script khởi chạy đồng thời toàn bộ 8 services
└── src/
    ├── ApiGateway/                  # [Port 5000] Ocelot Gateway, định tuyến & kiểm tra JWT
    ├── MiniMart.Shared/             # Thư viện dùng chung (ApiResponse<T>, PagedResponse, JWT, Middleware)
    ├── AuthService/                 # [Port 5001] Đăng nhập, đăng ký, JWT, phân quyền User & Role
    ├── ProductService/              # [Port 5002] Quản lý Sản phẩm, Danh mục, Nhà cung cấp, Barcode
    ├── InventoryService/            # [Port 5003] Quản lý Tồn kho, Phiếu nhập, Cảnh báo, Trừ/Hoàn kho
    ├── PromotionService/            # [Port 5004] Quản lý Chương trình khuyến mãi, Tính giảm giá
    ├── OrderService/                # [Port 5005] Đơn hàng, Chi tiết đơn hàng, Thanh toán, Điều phối luồng SOA
    ├── CustomerService/             # [Port 5006] Quản lý Khách hàng, Điểm tích lũy, Phân hạng thành viên
    └── ReportService/               # [Port 5007] Thống kê Doanh thu, Sản phẩm bán chạy, Tồn kho
```

### Mỗi Service đều tuân theo chuẩn kiến trúc phân lớp:
- `Controllers/` - REST API Endpoints
- `Services/` - Nghiệp vụ và điều phối liên service
- `Repositories/` - Xử lý truy vấn dữ liệu
- `DTOs/` - Đối tượng truyền tải dữ liệu
- `Models/` - Các thực thể nghiệp vụ (Entities)
- `Data/` - DbContext và cấu hình ánh xạ Entity Framework Core
- `Migrations/` - Lịch sử migration cơ sở dữ liệu

---

## 2. Danh mục Cổng (Ports) & Swagger

| Service | Port | Swagger URL |
| :--- | :---: | :--- |
| **API Gateway** | `5000` | `http://localhost:5000` |
| **Auth / User Service** | `5001` | `http://localhost:5001/swagger` |
| **Product Service** | `5002` | `http://localhost:5002/swagger` |
| **Inventory Service** | `5003` | `http://localhost:5003/swagger` |
| **Promotion Service** | `5004` | `http://localhost:5004/swagger` |
| **Order / Sales Service** | `5005` | `http://localhost:5005/swagger` |
| **Customer Service** | `5006` | `http://localhost:5006/swagger` |
| **Report Service** | `5007` | `http://localhost:5007/swagger` |

---

## 3. Hướng dẫn chạy Backend

### Cách 1: Chạy toàn bộ hệ thống bằng PowerShell script
```powershell
cd backend
.\run-backend.ps1
```

### Cách 2: Chạy riêng từng service
```powershell
cd backend/src/ApiGateway
dotnet run
```
