# HỆ THỐNG QUẢN LÝ SIÊU THỊ MINI (MINIMART)
### Học phần: Kiến trúc hướng dịch vụ (SOA) — Năm học 2026 – 2027
**Trường Đại học Quy Nhơn — Khoa Công nghệ Thông tin**  
**Giảng viên hướng dẫn:** Võ Thị Mỹ

---

## 👥 1. Thành viên nhóm & Phân công nhiệm vụ

| STT | Họ và tên | Vai trò / Phụ trách | Phạm vi dịch vụ |
| :-: | :--- | :--- | :--- |
| 1 | **Phan Nguyễn Gia Huy** | Trưởng nhóm / Kiến trúc | Tổng quan, API Gateway, Kiến trúc SOA, Điều phối tổng hợp |
| 2 | **Hồ Minh Chí** | Thành viên | **Product Service** (Sản phẩm, danh mục, nhà cung cấp, barcode) |
| 3 | **Trần Thị Mỹ Thu** | Thành viên | **Inventory Service** (Tồn kho, nhập kho) & **Promotion Service** (Khuyến mãi) |
| 4 | **Võ Thị Kiều Trang** | Thành viên | **Order Service** (Đơn hàng, chi tiết đơn, thanh toán, điều phối luồng mua hàng) |
| 5 | **Trần Bảo Việt** | Thành viên | **Customer Service** (Khách hàng, tích điểm) & **Report Service** (Báo cáo thống kê) |

---

## 🏛️ 2. Kiến trúc tổng thể hệ thống (SOA)

Hệ thống được thiết kế theo kiến trúc **Hướng dịch vụ (SOA - Service-Oriented Architecture)** phân tán:
- **Client (Frontend Angular):** Giao diện Single Page Application (SPA), chỉ kết nối tập trung qua **API Gateway**, không gọi trực tiếp database hay các service nội bộ.
- **API Gateway (Ocelot):** Đóng vai trò Reverse Proxy, điểm vào duy nhất (Single Point of Entry), định tuyến request, xác thực JWT và cấu hình CORS.
- **7 Microservices độc lập:** Mỗi service sở hữu cơ sở dữ liệu riêng (Database-per-service pattern), giao tiếp với nhau qua HTTP/RESTful API (`IHttpClientFactory`).
- **Thư viện dùng chung (`MiniMart.Shared`):** Chuẩn hóa định dạng phản hồi `ApiResponse<T>`, `PagedResponse<T>`, Global Exception Middleware và xác thực JWT Bearer dùng chung.

```text
                  ┌─────────────────────────────────────────┐
                  │        Client (Angular 19+ SPA)         │
                  │         http://localhost:4200           │
                  └────────────────────┬────────────────────┘
                                       │ HTTP / JSON + JWT
                                       ▼
                  ┌─────────────────────────────────────────┐
                  │       API Gateway (Ocelot - C#)         │
                  │         http://localhost:5000           │
                  └───────┬─────────────────────────┬───────┘
                          │                         │
     ┌────────────────────┼─────────────────────────┼────────────────────┐
     ▼                    ▼                         ▼                    ▼
┌──────────────┐   ┌──────────────┐          ┌──────────────┐   ┌──────────────┐
│ Auth/User    │   │ Product      │          │ Inventory    │   │ Promotion    │
│ Port: 5001   │   │ Port: 5002   │          │ Port: 5003   │   │ Port: 5004   │
└──────┬───────┘   └──────┬───────┘          └──────┬───────┘   └──────┬───────┘
       ▼                  ▼                         ▼                    ▼
  [AuthDb]           [ProductDb]               [InventoryDb]       [PromotionDb]

     ┌────────────────────┼─────────────────────────┼────────────────────┐
     ▼                    ▼                         ▼                    ▼
┌──────────────┐   ┌──────────────┐          ┌──────────────┐   ┌──────────────┐
│ Order/Sales  │   │ Customer     │          │ Report       │   │ MiniMart     │
│ Port: 5005   │   │ Port: 5006   │          │ Port: 5007   │   │ .Shared      │
└──────┬───────┘   └──────┬───────┘          └──────┬───────┘   └──────────────┘
       ▼                  ▼                         │
   [OrderDb]         [CustomerDb]          (Đọc dữ liệu qua API)
```

---

## 📂 3. Cấu trúc thư mục dự án

```text
D:\SOA\MiniMart\
├── backend/                                # Toàn bộ giải pháp Backend (.NET 9)
│   ├── MiniMart.sln                        # Visual Studio Solution chứa 9 projects
│   ├── run-backend.ps1                     # Script khởi chạy đồng thời toàn bộ 8 services
│   └── src/
│       ├── ApiGateway/                     # [Port 5000] Ocelot Gateway
│       ├── MiniMart.Shared/                # Thư viện dùng chung (Response, JWT, Middleware)
│       ├── AuthService/                    # [Port 5001] Quản lý User, Phân quyền & JWT
│       ├── ProductService/                 # [Port 5002] Quản lý Sản phẩm, Danh mục, NCC
│       ├── InventoryService/               # [Port 5003] Quản lý Tồn kho, Phiếu nhập
│       ├── PromotionService/               # [Port 5004] Quản lý Khuyến mãi, Giảm giá
│       ├── OrderService/                   # [Port 5005] Đơn hàng, Thanh toán, Điều phối SOA
│       ├── CustomerService/                # [Port 5006] Khách hàng, Điểm tích lũy, Hạng thẻ
│       └── ReportService/                  # [Port 5007] Thống kê Doanh thu, Xuất báo cáo
│
└── frontend/                               # Toàn bộ mã nguồn Frontend (Angular 19+)
    ├── src/
    │   ├── app/
    │   │   ├── core/                       # Auth Interceptors, Guards, API Services
    │   │   ├── shared/                     # Components dùng chung (Header, Footer, Modals)
    │   │   └── features/                   # Các màn hình chức năng:
    │   │       ├── auth/                   # Đăng nhập, Đăng ký
    │   │       ├── products/               # Danh mục & Danh sách sản phẩm
    │   │       ├── pos/                    # Bán hàng tại quầy (POS Checkout)
    │   │       ├── inventory/              # Quản lý kho hàng & Cảnh báo tồn
    │   │       └── reports/                # Dashboard biểu đồ & Báo cáo doanh thu
    ├── package.json
    └── angular.json
```

> **Ghi chú về kiến trúc từng Service:**  
> Mỗi service trong `backend/src/` được chia thành các tầng chuẩn:
> - `Controllers/`: Tiếp nhận HTTP Request, validate tham số và trả về `ApiResponse<T>`.
> - `Services/`: Xử lý nghiệp vụ logic và gọi API các service liên quan.
> - `Repositories/`: Tầng giao tiếp và xử lý dữ liệu.
> - `DTOs/`: Đối tượng truyền nhận dữ liệu qua API.
> - `Models/`: Các bảng / thực thể cơ sở dữ liệu.
> - `Data/`: Cấu hình EF Core DbContext và Seed dữ liệu ban đầu.
> - `Migrations/`: Lịch sử di chuyển sơ đồ cơ sở dữ liệu.

---

## 🌐 4. Danh mục Cổng mạng (Ports) & Swagger API

| STT | Tên Service | Port | Đường dẫn Swagger Docs | Phụ trách chính |
| :-: | :--- | :---: | :--- | :--- |
| 1 | **API Gateway** | `5000` | `http://localhost:5000` | Phan Nguyễn Gia Huy |
| 2 | **Auth / User Service** | `5001` | `http://localhost:5001/swagger` | Phan Nguyễn Gia Huy |
| 3 | **Product Service** | `5002` | `http://localhost:5002/swagger` | Hồ Minh Chí |
| 4 | **Inventory Service** | `5003` | `http://localhost:5003/swagger` | Trần Thị Mỹ Thu |
| 5 | **Promotion Service** | `5004` | `http://localhost:5004/swagger` | Trần Thị Mỹ Thu |
| 6 | **Order / Sales Service** | `5005` | `http://localhost:5005/swagger` | Võ Thị Kiều Trang |
| 7 | **Customer Service** | `5006` | `http://localhost:5006/swagger` | Trần Bảo Việt |
| 8 | **Report Service** | `5007` | `http://localhost:5007/swagger` | Trần Bảo Việt |

---

## 🔑 5. Tài khoản mẫu để kiểm thử (Seed Accounts)

Hệ thống đã được thiết lập sẵn các tài khoản demo tương ứng với 4 vai trò chính:

| Vai trò | Tên đăng nhập | Mật khẩu | Quyền hạn chính |
| :--- | :--- | :--- | :--- |
| **Quản trị viên (Admin)** | `admin` | `admin123` | Toàn quyền quản trị hệ thống, sản phẩm, nhân sự, báo cáo |
| **Thu ngân (Cashier)** | `cashier` | `cashier123` | Bán hàng POS tại quầy, tra cứu sản phẩm/khách hàng, thanh toán |
| **Thủ kho (Stockkeeper)** | `stockkeeper` | `stock123` | Lập phiếu nhập kho, điều chỉnh tồn kho, kiểm tra cảnh báo thiếu hàng |
| **Khách hàng (Customer)** | `customer` | `customer123` | Đặt hàng trực tuyến nhận tại quầy, xem lịch sử mua sắm & tích điểm |

---

## 🚀 6. Hướng dẫn cài đặt & Khởi chạy hệ thống

### 6.1 Khởi chạy Backend (.NET 9)

#### Cách 1: Khởi động tự động toàn bộ 8 services (Khuyến nghị)
Mở PowerShell tại thư mục gốc của dự án:
```powershell
cd D:\SOA\MiniMart\backend
.\run-backend.ps1
```
*Script sẽ mở đồng thời các cửa sổ riêng biệt cho từng service và API Gateway.*

#### Cách 2: Biên dịch và chạy thủ công
1. **Biên dịch toàn bộ solution:**
   ```powershell
   cd D:\SOA\MiniMart\backend
   dotnet build
   ```
2. **Khởi chạy API Gateway:**
   ```powershell
   cd D:\SOA\MiniMart\backend\src\ApiGateway
   dotnet run
   ```
3. **Khởi chạy từng service cần làm việc** :
   ```powershell
   cd D:\SOA\MiniMart\backend\src\ProductService
   dotnet run
   ```

---

### 6.2 Khởi chạy Frontend (Angular)

1. **Cài đặt thư viện dependencies (chỉ thực hiện ở lần đầu tiên):**
   ```powershell
   cd D:\SOA\MiniMart\frontend
   npm install
   ```

2. **Khởi chạy Development Server:**
   ```powershell
   npm start
   # hoặc:
   npx ng serve -o
   ```
*Trình duyệt sẽ tự động mở ứng dụng tại: `http://localhost:4200`.*

---

## 📡 7. Quy ước chuẩn giao tiếp API

- **Giao thức:** RESTful API qua HTTP/JSON (chuẩn `camelCase`).
- **Xác thực:** JWT Bearer Token gửi trong Header: `Authorization: Bearer <Token>`.
- **Cấu trúc phản hồi thống nhất (`ApiResponse<T>`):**
  ```json
  {
    "success": true,
    "message": "Thao tác thành công",
    "data": { ... },
    "errors": null
  }
  ```
- **Xử lý phân trang (`PagedResponse<T>`):** `?page=1&pageSize=20`.
- **Cơ chế hoàn tác giao dịch phân tán (Rollback):** Khi tạo đơn hàng xảy ra lỗi sau bước trừ tồn kho, OrderService tự động gửi yêu cầu bù trừ (`POST /api/inventory/rollback`) để bảo toàn tính nhất quán dữ liệu giữa các microservices.
