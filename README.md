# MiniMart Enterprise Solution

Dự án **MiniMart** được xây dựng theo kiến trúc Service-Oriented Architecture (SOA) / Microservices phân tán cho Backend và Angular Single Page Application (SPA) chuẩn Enterprise cho Frontend.

---

## 1. Cấu trúc thư mục (Directory Structure)

```text
D:\SOA\MiniMart\
├── backend/
│   ├── MiniMart.sln                   # Visual Studio / .NET Solution
│   └── src/
│       ├── MiniMart.Shared/           # Thư viện dùng chung (DTOs, Base Models, Constants)
│       ├── AuthService/               # Service xác thực, phân quyền (JWT, User Auth)
│       ├── ProductService/            # Service quản lý danh mục, sản phẩm, tồn kho
│       └── OrderService/              # Service xử lý đơn hàng, giỏ hàng, thanh toán
│
└── frontend/
    └── minimart-client/               # Angular 19+ SPA (Standalone Components, SCSS)
        ├── src/
        │   ├── app/
        │   │   ├── core/              # Singleton Services, Auth Interceptor, Guards
        │   │   ├── shared/            # Reusable UI Components, Pipes, Directives
        │   │   └── features/          # Feature Modules / Pages:
        │   │       ├── auth/          # Login, Register
        │   │       ├── products/      # Product Catalog, Details
        │   │       └── orders/        # Cart, Checkout, Order History
        │   ├── styles.scss
        │   └── main.ts
        ├── angular.json
        └── package.json
```

---

## 2. Hướng dẫn chạy Backend (.NET 9)

### Build toàn bộ hệ thống Backend:
```powershell
cd D:\SOA\MiniMart\backend
dotnet build
```

### Chạy từng dịch vụ:
- **AuthService**:
  ```powershell
  cd D:\SOA\MiniMart\backend\src\AuthService
  dotnet run
  ```
- **ProductService**:
  ```powershell
  cd D:\SOA\MiniMart\backend\src\ProductService
  dotnet run
  ```
- **OrderService**:
  ```powershell
  cd D:\SOA\MiniMart\backend\src\OrderService
  dotnet run
  ```

---

## 3. Hướng dẫn chạy Frontend (Angular)

### Cài đặt dependencies (lần đầu tiên):
```powershell
cd D:\SOA\MiniMart\frontend\minimart-client
npm install
```

### Khởi chạy Development Server:
```powershell
npm start
# hoặc
npx ng serve -o
```
Ứng dụng sẽ tự động mở tại `http://localhost:4200/`.
