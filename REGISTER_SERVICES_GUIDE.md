# 📝 Hướng dẫn Đăng ký Services & Repositories

## ⚙️ Cập nhật Program.cs

Thêm các dòng sau vào file `PRN222_FinalProject/Program.cs`:

```csharp
// ============================================
// REPOSITORIES (Data Access Layer)
// ============================================

// Existing repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<IBidRepository, BidRepository>();

// NEW - 7 Steps Features Repositories
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();

// ============================================
// SERVICES (Business Logic Layer)
// ============================================

// Existing services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// NEW - 7 Steps Features Services
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// TODO - Implement these services later
// builder.Services.AddScoped<IMessageService, MessageService>();
// builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
```

---

## 📋 Checklist Đăng ký

### ✅ Repositories (DAL)
- [x] IProductRepository
- [x] IOrderRepository
- [x] ICategoryRepository
- [x] IUserRepository
- [x] IWalletRepository
- [x] IAuctionRepository
- [x] IBidRepository
- [x] IFavoriteRepository ← NEW
- [x] IReviewRepository ← NEW
- [x] IMessageRepository ← NEW
- [x] ISupportTicketRepository ← NEW

### ✅ Services (BLL)
- [x] IProductService
- [x] IOrderService
- [x] ICategoryService
- [x] IUserService
- [x] IWalletService
- [x] IAuctionService
- [x] IContractService
- [x] IAdminService
- [x] IDeliveryService ← NEW
- [x] IFavoriteService ← NEW
- [x] IReviewService ← NEW
- [ ] IMessageService ← TODO
- [ ] ISupportTicketService ← TODO

---

## 🔍 Kiểm tra Đăng ký

### Test 1: Build Project
```bash
dotnet build
```
Nếu có lỗi "Unable to resolve service" → Thiếu đăng ký

### Test 2: Run Project
```bash
dotnet run --project PRN222_FinalProject
```
Nếu runtime error → Kiểm tra constructor dependencies

### Test 3: Check DI Container
Thêm vào một page để test:
```csharp
public class TestModel : PageModel
{
    private readonly IFavoriteService _favoriteService;
    private readonly IReviewService _reviewService;
    
    public TestModel(
        IFavoriteService favoriteService,
        IReviewService reviewService)
    {
        _favoriteService = favoriteService;
        _reviewService = reviewService;
    }
    
    public void OnGet()
    {
        // If this page loads without error, DI is working!
    }
}
```

---

## 🚨 Common Errors

### Error 1: "Unable to resolve service for type 'IFavoriteRepository'"
**Nguyên nhân:** Chưa đăng ký repository

**Giải pháp:**
```csharp
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
```

### Error 2: "A circular dependency was detected"
**Nguyên nhân:** Service A inject Service B, Service B inject Service A

**Giải pháp:** Refactor để tránh circular dependency

### Error 3: "Cannot consume scoped service from singleton"
**Nguyên nhân:** Singleton service inject Scoped service

**Giải pháp:** Đổi tất cả thành Scoped hoặc dùng IServiceScopeFactory

---

## 📊 Service Lifetime

### Scoped (Recommended)
```csharp
builder.Services.AddScoped<IProductService, ProductService>();
```
- Tạo mới mỗi HTTP request
- Phù hợp cho web applications
- **Sử dụng cho tất cả Services & Repositories**

### Transient
```csharp
builder.Services.AddTransient<IEmailService, EmailService>();
```
- Tạo mới mỗi lần inject
- Phù hợp cho lightweight, stateless services

### Singleton
```csharp
builder.Services.AddSingleton<IConfiguration>(configuration);
```
- Chỉ tạo 1 instance duy nhất
- Phù hợp cho configuration, caching

**⚠️ Lưu ý:** DbContext PHẢI là Scoped!

---

## ✅ Complete Program.cs Example

```csharp
using DAL.Entities;
using DAL.Repositories;
using BLL.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EvBatteryTrading2Context>(options =>
    options.UseSqlServer(connectionString));

// ============================================
// REPOSITORIES (Data Access Layer)
// ============================================
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();

// ============================================
// SERVICES (Business Logic Layer)
// ============================================
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();
app.MapRazorPages();

app.Run();
```

---

## 🎯 Next Steps

1. ✅ Thêm registrations vào Program.cs
2. ✅ Build project
3. ✅ Run project
4. ✅ Test một vài pages
5. ✅ Verify không có DI errors

---

## 📞 Troubleshooting

Nếu gặp lỗi, kiểm tra:

1. **Namespace đúng chưa?**
   ```csharp
   using DAL.Repositories;
   using BLL.Services;
   ```

2. **Interface và Implementation match chưa?**
   ```csharp
   // Interface
   public interface IFavoriteService { }
   
   // Implementation
   public class FavoriteService : IFavoriteService { }
   ```

3. **Constructor dependencies đúng chưa?**
   ```csharp
   public class FavoriteService : IFavoriteService
   {
       private readonly IFavoriteRepository _repository;
       
       public FavoriteService(IFavoriteRepository repository)
       {
           _repository = repository;
       }
   }
   ```

4. **Tất cả dependencies đã được đăng ký chưa?**
   - Nếu Service inject Repository → Repository phải được đăng ký trước
