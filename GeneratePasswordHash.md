# Generate Password Hash

## Cách 1: Sử dụng C# Console App

Tạo một console app nhỏ để generate BCrypt hash:

```csharp
using BCrypt.Net;

Console.WriteLine("Password Hash Generator");
Console.WriteLine("======================");

// Admin@123
string adminPassword = "Admin@123";
string adminHash = BCrypt.HashPassword(adminPassword);
Console.WriteLine($"Admin@123 hash: {adminHash}");

// User@123
string userPassword = "User@123";
string userHash = BCrypt.HashPassword(userPassword);
Console.WriteLine($"User@123 hash: {userHash}");

Console.WriteLine("\nCopy these hashes to SeedData.sql");
```

## Cách 2: Sử dụng Online BCrypt Generator

1. Truy cập: https://bcrypt-generator.com/
2. Nhập password: `Admin@123`
3. Rounds: 11
4. Copy hash và paste vào SQL script

## Cách 3: Đăng ký trực tiếp qua Web

1. Chạy ứng dụng
2. Truy cập `/Account/Register`
3. Đăng ký tài khoản với thông tin mong muốn
4. Password sẽ tự động được hash

## Cách 4: Sử dụng SQL Script với .NET

Tạo một migration hoặc seed data trong EF Core:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    modelBuilder.Entity<User>().HasData(
        new User
        {
            Id = 1,
            Email = "admin@evbattery.com",
            Phone = "0901234567",
            FullName = "Admin System",
            Address = "Hà Nội, Việt Nam",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "admin",
            IsVerified = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        }
    );
}
```

## Recommended Approach

**Đăng ký trực tiếp qua Web UI** là cách đơn giản và an toàn nhất:

1. Chạy ứng dụng: `dotnet run`
2. Truy cập: https://localhost:5001
3. Click "Đăng ký"
4. Điền thông tin:
   - Email: admin@evbattery.com
   - Phone: 0901234567
   - Full Name: Admin System
   - Password: Admin@123
   - Confirm Password: Admin@123
5. Sau khi đăng ký, vào SQL Server và update role:
   ```sql
   UPDATE users 
   SET role = 'admin', is_verified = 1 
   WHERE email = 'admin@evbattery.com';
   ```

## Sample Hashes (Generated with BCrypt Rounds=11)

**Note**: Mỗi lần generate sẽ ra hash khác nhau do BCrypt tự động thêm salt.

Ví dụ hash cho `Admin@123`:
```
$2a$11$XvZ5YqJ5YqJ5YqJ5YqJ5YuJ5YqJ5YqJ5YqJ5YqJ5YqJ5YqJ5YqJ5Y
```

Ví dụ hash cho `User@123`:
```
$2a$11$YvZ5YqJ5YqJ5YqJ5YqJ5YuJ5YqJ5YqJ5YqJ5YqJ5YqJ5YqJ5YqJ5Z
```

**Important**: Các hash trên chỉ là ví dụ. Bạn cần generate hash thực tế.
