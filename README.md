# EV Battery Trading Platform - Chủ đề 1: Quản lý Tài khoản

## Tổng quan
Nền tảng giao dịch pin xe điện thứ hai (Second-hand EV & Battery Trading Platform) - Chủ đề 1: Quản lý Tài khoản.

Dự án được xây dựng theo kiến trúc 3 layer:
- **DAL (Data Access Layer)**: Quản lý Entities, Repositories, DTOs
- **BLL (Business Logic Layer)**: Quản lý Services, Business Logic
- **Presentation Layer**: ASP.NET Core Razor Pages

## Công nghệ sử dụng
- .NET 8.0
- ASP.NET Core Razor Pages
- Entity Framework Core 8.0.7
- SQL Server
- BCrypt.Net-Next 4.0.3 (Password Hashing)
- Bootstrap 5 + Bootstrap Icons
- Session-based Authentication

## Cấu trúc dự án

```
PRN222_FinalProject/
├── DAL/                          # Data Access Layer
│   ├── Entities/                 # Entity classes (User, Product, Order, etc.)
│   ├── Repositories/             # Repository interfaces & implementations
│   │   ├── IUserRepository.cs
│   │   └── UserRepository.cs
│   └── DTOs/                     # Data Transfer Objects
│       ├── RegisterDto.cs
│       ├── LoginDto.cs
│       ├── UpdateProfileDto.cs
│       ├── ChangePasswordDto.cs
│       ├── UserDto.cs
│       └── AdminUserManagementDto.cs
│
├── BLL/                          # Business Logic Layer
│   ├── Services/                 # Service interfaces & implementations
│   │   ├── IAuthService.cs
│   │   ├── AuthService.cs
│   │   ├── IUserService.cs
│   │   ├── UserService.cs
│   │   ├── IAdminService.cs
│   │   └── AdminService.cs
│   └── Helpers/
│       └── SessionHelper.cs
│
└── PRN222_FinalProject/          # Presentation Layer
    ├── Pages/
    │   ├── Account/              # User account pages
    │   │   ├── Register.cshtml
    │   │   ├── Login.cshtml
    │   │   ├── Logout.cshtml
    │   │   └── Profile.cshtml
    │   ├── Admin/                # Admin pages
    │   │   ├── Dashboard.cshtml
    │   │   └── Users/
    │   │       ├── Index.cshtml
    │   │       ├── Create.cshtml
    │   │       └── Edit.cshtml
    │   └── Index.cshtml
    ├── Program.cs
    └── appsettings.json
```

## Chức năng đã triển khai (Chủ đề 1)

### 1. Đăng ký tài khoản mới (Use Case #1)
- **Actors**: Guest
- **Chức năng**:
  - Nhập thông tin: Email, Phone, Full Name, Address, Password
  - Validation đầy đủ (Email unique, Phone unique, Password strength)
  - Hash password với BCrypt
  - Tạo tài khoản với role "member"
  - Trạng thái mặc định: IsVerified = false (cần xác thực)
- **Bảo mật**: 
  - Password hashing với BCrypt
  - Validation đầy đủ
  - Rate limiting (có thể mở rộng)

### 2. Đăng nhập hệ thống (Use Case #2)
- **Actors**: Guest
- **Chức năng**:
  - Đăng nhập bằng Email và Password
  - Xác thực password với BCrypt
  - Kiểm tra tài khoản đã xác thực (IsVerified)
  - Lưu thông tin user vào Session
  - Redirect theo role (Admin → Dashboard, Member → Home)
- **Bảo mật**:
  - Lock account sau 5 lần đăng nhập sai (60 phút)
  - Session-based authentication
  - Remember Me option (24h hoặc 30 ngày)

### 3. Quản lý thông tin cá nhân (Use Case #3)
- **Actors**: Member
- **Chức năng**:
  - Xem profile (Email, Full Name, Phone, Address, Created Date)
  - Cập nhật thông tin (Full Name, Phone, Address)
  - Đổi mật khẩu (Current Password, New Password)
  - Validation phone unique khi thay đổi
- **Audit Trail**: 
  - Lưu UpdatedAt timestamp
  - Log thay đổi (có thể mở rộng)

### 4. Quản lý tài khoản (Admin) (Use Case #4)
- **Actors**: Admin
- **Chức năng**:
  - Liệt kê tất cả users (với pagination)
  - Tìm kiếm users (Email, Name, Phone)
  - Tạo user mới (Admin có thể set role và IsVerified)
  - Chỉnh sửa user (Email, Phone, Name, Address, Role, IsVerified)
  - Suspend/Activate user (soft delete)
  - Xóa user (không thể xóa Admin)
  - Dashboard với thống kê
- **Phân quyền**: 
  - Role-Based Access Control
  - Chỉ Admin mới truy cập được
  - Không thể suspend/delete Admin

## Cài đặt và chạy dự án

### Yêu cầu
- .NET 8.0 SDK
- SQL Server (local hoặc remote)
- Visual Studio 2022 hoặc VS Code

### Bước 1: Clone/Download project

### Bước 2: Cấu hình Database
1. Mở SQL Server Management Studio
2. Tạo database `ev_battery_trading2` (hoặc sử dụng database có sẵn)
3. Cập nhật connection string trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Uid=sa;Pwd=YOUR_PASSWORD;Database=ev_battery_trading2;TrustServerCertificate=True"
  }
}
```

### Bước 3: Restore packages
```bash
cd PRN222_FinalProject
dotnet restore
```

### Bước 4: Build project
```bash
dotnet build
```

### Bước 5: Run project
```bash
cd PRN222_FinalProject
dotnet run
```

Hoặc sử dụng Visual Studio: F5 để chạy

### Bước 6: Truy cập ứng dụng
- URL: https://localhost:5001 hoặc http://localhost:5000
- Đăng ký tài khoản mới hoặc sử dụng tài khoản admin có sẵn (nếu đã seed data)

## Tài khoản mẫu (nếu đã seed data)
- **Admin**: 
  - Email: admin@evbattery.com
  - Password: Admin@123
- **Member**:
  - Email: user@evbattery.com
  - Password: User@123

## Kiến trúc 3 Layer

### DAL (Data Access Layer)
- **Entities**: Định nghĩa các entity classes (User, Product, Order, etc.)
- **DbContext**: EvBatteryTrading2Context - quản lý kết nối database
- **Repositories**: Implement Repository Pattern
  - IUserRepository/UserRepository: CRUD operations cho User
- **DTOs**: Data Transfer Objects để transfer data giữa các layer

### BLL (Business Logic Layer)
- **Services**: Implement business logic
  - AuthService: Xử lý đăng ký, đăng nhập, hash password
  - UserService: Quản lý user profile, đổi mật khẩu
  - AdminService: Quản lý users (CRUD, suspend, activate)
- **Helpers**: SessionHelper - extension methods cho Session

### Presentation Layer
- **Razor Pages**: UI components
- **Session Management**: Lưu trữ user info
- **Bootstrap 5**: Responsive UI
- **Bootstrap Icons**: Icon library

## Bảo mật

### Password Security
- BCrypt hashing với salt tự động
- Password strength validation (chữ hoa, chữ thường, số)
- Minimum 6 characters

### Authentication
- Session-based authentication
- Session timeout: 24 hours (có thể extend với Remember Me)
- HttpOnly cookies

### Authorization
- Role-based access control (Admin, Member)
- Page-level authorization checks
- Admin pages chỉ accessible bởi Admin role

### Login Security
- Lock account sau 5 lần đăng nhập sai
- Lockout duration: 60 phút
- Memory cache để track login attempts

### Data Validation
- Server-side validation với Data Annotations
- Client-side validation với jQuery Validation
- Unique constraints (Email, Phone)

## Mở rộng tương lai

### Chủ đề 2: Quản lý Sản phẩm
- Đăng sản phẩm (pin/battery)
- Tìm kiếm, lọc sản phẩm
- Xem chi tiết sản phẩm
- Quản lý sản phẩm của seller

### Chủ đề 3: Giao dịch
- Giỏ hàng
- Checkout
- Thanh toán (VNPAY/MoMo integration)
- Lịch sử giao dịch

### Chủ đề 4: Admin Advanced
- Dashboard với charts
- Reports
- Email notifications
- Audit logs

## Troubleshooting

### Lỗi kết nối database
- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string trong appsettings.json
- Kiểm tra username/password

### Lỗi build
- Chạy `dotnet restore` để restore packages
- Kiểm tra .NET 8.0 SDK đã cài đặt

### Session không hoạt động
- Kiểm tra `app.UseSession()` trong Program.cs
- Kiểm tra Session configuration

## Liên hệ
- Email: support@evbattery.com
- GitHub: [Your GitHub URL]

## License
MIT License
