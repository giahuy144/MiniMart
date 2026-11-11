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

### Database MySQL và dữ liệu mẫu

Script khởi động sẽ tự bỏ qua service nào đã có tiến trình chiếm cổng. Nếu chạy service thủ công khi service cũ vẫn còn hoạt động, hãy dừng service cũ trước để tránh lỗi `address already in use`. File SQL tạo schema và chèn seed mẫu nằm tại `database/minimart_dev.sql` (chạy trong phpMyAdmin hoặc MySQL client).

Sáu service dữ liệu mặc định dùng MySQL tại `127.0.0.1:3306`, lần lượt kết nối các database `minimart_dev_auth`, `minimart_dev_product`, `minimart_dev_inventory`, `minimart_dev_promotion`, `minimart_dev_order` và `minimart_dev_customer`. Bộ database `minimart_*` cũ được giữ nguyên vì schema của chúng khác với entity trong code hiện tại. Cấu hình mặc định dùng user `root` không mật khẩu (thiết lập XAMPP phổ biến). Nếu MySQL của bạn có mật khẩu, sửa `Password=` trong `ConnectionStrings:DefaultConnection` ở `appsettings.json` của từng service.

Các seed mẫu có sẵn được giữ nguyên và được tạo khi service khởi tạo database trống. Để chạy thử bằng InMemory thay vì MySQL, mở PowerShell tại `backend` và chạy:

```powershell
.\run-backend.ps1 -DatabaseProvider InMemory
```

Dữ liệu InMemory chỉ tồn tại khi các service còn chạy. Script ghi log stdout/stderr theo cổng trong thư mục `backend/logs`; nếu cổng đã được service khác sử dụng, script sẽ giữ nguyên tiến trình đó và báo provider chỉ áp dụng cho service mới.
### Cách 2: Chạy riêng từng service
```powershell
cd backend/src/ApiGateway
dotnet run
```
