# Nền Tảng Giao Dịch Pin Xe Điện (EV Battery Trading Platform)

## Tổng quan
Nền tảng giao dịch pin xe điện thứ hai (Second-hand EV & Battery Trading Platform) là một hệ thống quản lý và giao dịch pin xe điện đã qua sử dụng. Dự án được phát triển nhằm tạo ra một thị trường trực tuyến minh bạch và hiệu quả cho việc mua bán, đấu giá pin xe điện.

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
- jQuery + AJAX (cho các tương tác không đồng bộ)
- Chart.js (cho báo cáo và thống kê)

## Cấu trúc dự án

```
PRN222_FinalProject/
├── DAL/                          # Data Access Layer
│   ├── Entities/                 # Entity classes
│   │   ├── User.cs
│   │   ├── Product.cs
│   │   ├── Category.cs
│   │   ├── Auction.cs
│   │   └── ...
│   │
│   ├── Repositories/             # Repository interfaces & implementations
│   │   ├── IUserRepository.cs
│   │   ├── UserRepository.cs
│   │   ├── IProductRepository.cs
│   │   ├── ProductRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   ├── CategoryRepository.cs
│   │   ├── IAuctionRepository.cs
│   │   └── AuctionRepository.cs
│   │
│   └── DTOs/                     # Data Transfer Objects
│       ├── Account/
│       │   ├── RegisterDto.cs
│       │   ├── LoginDto.cs
│       │   ├── UpdateProfileDto.cs
│       │   ├── ChangePasswordDto.cs
│       │   └── UserDto.cs
│       ├── Product/
│       │   ├── ProductDto.cs
│       │   └── ...
│       └── Auction/
│           ├── AuctionDto.cs
│           └── ...
│
├── BLL/                          # Business Logic Layer
│   ├── Services/                 # Service interfaces & implementations
│   │   ├── Account/
│   │   │   ├── IAuthService.cs
│   │   │   ├── AuthService.cs
│   │   │   ├── IUserService.cs
│   │   │   └── UserService.cs
│   │   │
│   │   ├── Admin/
│   │   │   ├── IAdminService.cs
│   │   │   └── AdminService.cs
│   │   │
│   │   ├── Product/
│   │   │   ├── IProductService.cs
│   │   │   └── ProductService.cs
│   │   │
│   │   └── Auction/
│   │       ├── IAuctionService.cs
│   │       └── AuctionService.cs
│   │
│   └── Helpers/
│       ├── SessionHelper.cs
│       └── ...
│
└── PRN222_FinalProject/          # Presentation Layer
    ├── wwwroot/                 # Static files
    │   ├── css/
    │   ├── js/
    │   └── images/
    │
    ├── Pages/
    │   ├── Account/              # User account pages
    │   │   ├── Register.cshtml
    │   │   ├── Login.cshtml
    │   │   ├── Logout.cshtml
    │   │   └── Profile.cshtml
    │   │
    │   ├── Admin/                # Admin pages
    │   │   ├── Dashboard.cshtml
    │   │   └── Users/
    │   │       ├── Index.cshtml
    │   │       ├── Create.cshtml
    │   │       └── Edit.cshtml
    │   │
    │   ├── Staff/                # Staff pages
    │   │   ├── Dashboard/        # Trang tổng quan nhân viên
    │   │   │   └── Index.cshtml
    │   │   │
    │   │   ├── Products/         # Quản lý sản phẩm
    │   │   │   ├── Index.cshtml
    │   │   │   ├── Create.cshtml
    │   │   │   ├── Edit.cshtml
    │   │   │   └── Details.cshtml
    │   │   │
    │   │   ├── Categories/       # Quản lý danh mục
    │   │   │   ├── Index.cshtml
    │   │   │   ├── Create.cshtml
    │   │   │   └── Edit.cshtml
    │   │   │
    │   │   └── Auctions/         # Quản lý đấu giá
    │   │       ├── Index.cshtml
    │   │       ├── Create.cshtml
    │   │       └── Details.cshtml
    │   │
    │   └── Shared/               # Layout và components dùng chung
    │       ├── _Layout.cshtml
    │       ├── _LoginPartial.cshtml
    │       └── _ValidationScriptsPartial.cshtml
    │
    ├── appsettings.json
    ├── Program.cs
    └── _ViewImports.cshtml
```

## Các chức năng chính

### 1. Quản lý tài khoản (Account Management)

#### 1.1. Đăng ký tài khoản mới (Use Case #1)
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

#### 1.2. Đăng nhập hệ thống (Use Case #2)
- **Actors**: Guest
- **Chức năng**:
  - Đăng nhập bằng Email và Password
  - Xác thực password với BCrypt
  - Kiểm tra tài khoản đã xác thực (IsVerified)
  - Lưu thông tin user vào Session
  - Redirect theo role (Admin → Dashboard, Staff → Staff/Dashboard, Member → Home)
- **Bảo mật**:
  - Lock account sau 5 lần đăng nhập sai (60 phút)
  - Session-based authentication
  - Remember Me option (24h hoặc 30 ngày)

#### 1.3. Quản lý thông tin cá nhân (Use Case #3)
- **Actors**: Member, Staff, Admin
- **Chức năng**:
  - Xem profile (Email, Full Name, Phone, Address, Created Date)
  - Cập nhật thông tin (Full Name, Phone, Address)
  - Đổi mật khẩu (Current Password, New Password)
  - Validation phone unique khi thay đổi
- **Audit Trail**: 
  - Lưu UpdatedAt timestamp
  - Log thay đổi (có thể mở rộng)

#### 1.4. Quản lý tài khoản (Admin) (Use Case #4)
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

### 2. Quản lý sản phẩm (Staff/Admin)
- **Actors**: Staff, Admin
- **Chức năng chính**:
  - Xem danh sách sản phẩm (có phân trang, tìm kiếm, lọc theo danh mục)
  - Thêm mới sản phẩm (thông tin cơ bản, hình ảnh, mô tả, giá khởi điểm)
  - Chỉnh sửa thông tin sản phẩm
  - Xem chi tiết sản phẩm
  - Ẩn/hiện sản phẩm
  - Quản lý hình ảnh sản phẩm (thêm, xóa, sắp xếp)
- **Validation**:
  - Tên sản phẩm không được để trống
  - Giá khởi điểm phải lớn hơn 0
  - Danh mục phải tồn tại
  - Hình ảnh phải đúng định dạng

### 3. Quản lý danh mục (Staff/Admin)
- **Actors**: Staff, Admin
- **Chức năng chính**:
  - Xem danh sách danh mục (có phân trang)
  - Thêm mới danh mục
  - Chỉnh sửa thông tin danh mục
  - Ẩn/hiện danh mục
  - Sắp xếp thứ tự hiển thị danh mục
- **Validation**:
  - Tên danh mục không được trùng
  - Mô tả không quá 500 ký tự
  - Không thể xóa danh mục đang có sản phẩm

### 4. Quản lý đấu giá (Staff/Admin)
- **Actors**: Staff, Admin
- **Chức năng chính**:
  - Tạo phiên đấu giá mới
  - Quản lý trạng thái phiên đấu giá (đang mở, đã đóng, đã kết thúc)
  - Xem danh sách các phiên đấu giá
  - Xem chi tiết phiên đấu giá (bao gồm lịch sử đấu giá)
  - Kết thúc phiên đấu giá sớm (nếu cần)
  - Hủy phiên đấu giá (có lý do)
- **Tính năng nâng cao**:
  - Tự động gửi thông báo khi có người đấu giá cao hơn
  - Tự động cập nhật trạng thái phiên đấu giá khi hết hạn

### 5. Dashboard nhân viên (Staff Dashboard)
- **Tổng quan**:
  - Thống kê số lượng sản phẩm đang đấu giá
  - Số lượng đơn đấu giá đang chờ xử lý
  - Thống kê doanh thu theo ngày/tuần/tháng
  - Biểu đồ thống kê hoạt động đấu giá
  - Danh sách các phiên đấu giá sắp kết thúc
  - Thông báo mới (nếu có)

### 6. Phân quyền người dùng
- **Vai trò chính**:
  1. **Admin**: Toàn quyền truy cập hệ thống
  2. **Staff**: Quản lý sản phẩm, danh mục, đấu giá
  3. **Member**: Tham gia đấu giá, quản lý thông tin cá nhân
  4. **Guest**: Xem sản phẩm, tìm kiếm, đăng ký tài khoản

### 7. Báo cáo và thống kê (Admin/Staff)
- **Báo cáo doanh thu**:
  - Theo ngày/tuần/tháng/năm
  - Theo danh mục sản phẩm
  - Theo người bán/người mua
- **Thống kê hoạt động**:
  - Số lượng người dùng mới
  - Số lượng sản phẩm được đăng bán
  - Tỷ lệ thành công của các phiên đấu giá
  - Top sản phẩm được đấu giá nhiều nhất

## Hướng dẫn cài đặt và chạy dự án

### Yêu cầu hệ thống
- .NET 8.0 SDK
- SQL Server 2019 trở lên (local hoặc remote)
- Visual Studio 2022 (khuyến nghị) hoặc VS Code
- Git (để clone repository)

### Bước 1: Clone dự án
```bash
git clone <repository-url>
cd PRN222_FinalProject
```

### Bước 2: Cấu hình cơ sở dữ liệu
1. Mở SQL Server Management Studio (SSMS) hoặc Azure Data Studio
2. Tạo mới database với tên `ev_battery_trading2` hoặc sử dụng database có sẵn
3. Mở file `appsettings.json` và cập nhật connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server_name;Database=ev_battery_trading2;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Bước 3: Áp dụng migrations và khởi tạo dữ liệu mẫu
```bash
# Di chuyển đến thư mục dự án
cd PRN222_FinalProject

# Áp dụng migrations
dotnet ef database update
```

### Bước 4: Cài đặt các gói NuGet cần thiết
```bash
dotnet restore
```

### Bước 5: Build dự án
```bash
dotnet build
```

### Bước 6: Chạy ứng dụng
Có thể chạy bằng một trong các cách sau:

**Cách 1: Sử dụng .NET CLI**
```bash
dotnet run --project PRN222_FinalProject
```

**Cách 2: Sử dụng Visual Studio**
1. Mở file `PRN222_FinalProject.sln` bằng Visual Studio 2022
2. Nhấn F5 hoặc chọn "Start Debugging" từ menu Debug

### Bước 7: Truy cập ứng dụng
- URL mặc định: https://localhost:5001 hoặc http://localhost:5000
- Đăng nhập bằng tài khoản admin hoặc đăng ký tài khoản mới

### Bước 8: Tài khoản mẫu (nếu đã seed data)
- **Admin**: 
  - Email: admin@evbattery.com
  - Mật khẩu: Admin@123
  - Quyền: Toàn quyền hệ thống

- **Nhân viên (Staff)**:
  - Email: staff@evbattery.com
  - Mật khẩu: Staff@123
  - Quyền: Quản lý sản phẩm, danh mục, đấu giá

- **Người dùng (Member)**:
  - Email: user@evbattery.com
  - Mật khẩu: User@123
  - Quyền: Xem sản phẩm, tham gia đấu giá

## Tài khoản mẫu (nếu đã seed data)
- **Admin**: 
  - Email: admin@evbattery.com
  - Password: Admin@123
- **Member**:
  - Email: user@evbattery.com
  - Password: User@123

## Kiến trúc hệ thống

### 1. Kiến trúc 3 Layer

#### 1.1. Data Access Layer (DAL)
- **Entities**: Định nghĩa các đối tượng tương ứng với các bảng trong database
  - User: Thông tin người dùng
  - Product: Thông tin sản phẩm
  - Category: Danh mục sản phẩm
  - Auction: Thông tin đấu giá
  - Bid: Lịch sử đấu giá
  - ...và các entity khác

- **DbContext**: `EvBatteryTrading2Context`
  - Quản lý kết nối database
  - Cấu hình các bảng và mối quan hệ
  - Theo dõi thay đổi dữ liệu

- **Repositories**: Triển khai Repository Pattern
  - IUserRepository/UserRepository: Quản lý người dùng
  - IProductRepository/ProductRepository: Quản lý sản phẩm
  - ICategoryRepository/CategoryRepository: Quản lý danh mục
  - IAuctionRepository/AuctionRepository: Quản lý đấu giá

- **DTOs (Data Transfer Objects)**:
  - Đối tượng trung gian truyền dữ liệu giữa các tầng
  - Giảm thiểu dữ liệu không cần thiết
  - Tăng tính bảo mật

#### 1.2. Business Logic Layer (BLL)
- **Services**: Triển khai business logic
  - **Account Services**:
    - AuthService: Xác thực, đăng nhập, đăng ký
    - UserService: Quản lý thông tin người dùng
    - AdminService: Quản trị hệ thống
  
  - **Product Services**:
    - ProductService: Quản lý sản phẩm, danh mục
    - ImageService: Xử lý upload và quản lý hình ảnh
  
  - **Auction Services**:
    - AuctionService: Quản lý phiên đấu giá
    - BidService: Xử lý đấu giá
    - NotificationService: Gửi thông báo

- **Helpers**:
  - SessionHelper: Quản lý session
  - FileHelper: Xử lý file
  - EmailHelper: Gửi email
  - DateTimeHelper: Xử lý ngày giờ

#### 1.3. Presentation Layer
- **Razor Pages**:
  - Sử dụng Razor syntax để tạo giao diện động
  - Phân chia thành các khu vực chức năng rõ ràng
  - Tái sử dụng component với Partial Views

- **Layout & Components**:
  - _Layout.cshtml: Layout chung cho toàn bộ ứng dụng
  - _LoginPartial.cshtml: Hiển thị thông tin đăng nhập
  - _ValidationScriptsPartial.cshtml: Validation scripts
  - Các component tái sử dụng

- **Client-side Libraries**:
  - jQuery: Xử lý AJAX, DOM manipulation
  - Bootstrap 5: Responsive UI framework
  - Bootstrap Icons: Thư viện icon
  - Chart.js: Vẽ biểu đồ thống kê
  - DataTables: Xử lý bảng dữ liệu
  - SweetAlert2: Thông báo đẹp

### 2. Cơ chế bảo mật

#### 2.1. Xác thực (Authentication)
- Session-based authentication
- Cookie authentication
- JWT (cho API nếu có)

#### 2.2. Phân quyền (Authorization)
- Role-based access control (RBAC)
- Policy-based authorization
- Claims-based authorization

#### 2.3. Bảo mật dữ liệu
- Mã hóa mật khẩu với BCrypt
- Bảo vệ CSRF
- XSS prevention
- SQL injection prevention
- Input validation

### 3. Xử lý ngoại lệ (Exception Handling)
- Global exception handler
- Custom error pages
- Logging lỗi
- User-friendly error messages

### 4. Logging & Monitoring
- Ghi log các hoạt động quan trọng
- Theo dõi hiệu năng
- Cảnh báo lỗi

### 5. Tối ưu hiệu năng
- Caching dữ liệu
- Tối ưu truy vấn database
- Phân trang dữ liệu
- Nén và bundle CSS/JS
- Tối ưu hình ảnh

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
