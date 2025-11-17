# 🏗️ 3-Layer Architecture Guidelines

## 📋 Tổng quan

Hệ thống tuân thủ **3-Layer Architecture** (Kiến trúc 3 tầng):

```
┌─────────────────────────────────────┐
│   Presentation Layer (UI/Pages)    │  ← Razor Pages, Controllers
├─────────────────────────────────────┤
│   Business Logic Layer (BLL)       │  ← Services
├─────────────────────────────────────┤
│   Data Access Layer (DAL)           │  ← Repositories, Entities, DbContext
└─────────────────────────────────────┘
```

---

## 🎯 Nguyên tắc Cơ bản

### 1. **Separation of Concerns** (Tách biệt trách nhiệm)
- Mỗi layer chỉ làm một việc
- Không được vi phạm ranh giới giữa các layer
- Dependency chỉ đi một chiều: UI → BLL → DAL

### 2. **Dependency Rule** (Quy tắc phụ thuộc)
```
✅ ĐÚNG:
UI → Service → Repository → DbContext

❌ SAI:
UI → DbContext (bỏ qua Service & Repository)
Service → DbContext (bỏ qua Repository)
```

### 3. **Single Responsibility** (Trách nhiệm đơn lẻ)
- Repository: Chỉ truy cập database
- Service: Chỉ xử lý business logic
- Page/Controller: Chỉ xử lý UI logic

---

## 📁 Cấu trúc Thư mục

```
PRN222_FinalProject/
├── PRN222_FinalProject/          # Presentation Layer
│   ├── Pages/                    # Razor Pages
│   ├── Controllers/              # API Controllers (nếu có)
│   └── Program.cs                # DI Configuration
│
├── BLL/                          # Business Logic Layer
│   └── Services/                 # Business Services
│       ├── IProductService.cs
│       ├── ProductService.cs
│       ├── IOrderService.cs
│       ├── OrderService.cs
│       └── ...
│
└── DAL/                          # Data Access Layer
    ├── Entities/                 # Domain Models
    │   ├── Product.cs
    │   ├── Order.cs
    │   └── EvBatteryTrading2Context.cs
    │
    ├── Repositories/             # Data Access
    │   ├── IProductRepository.cs
    │   ├── ProductRepository.cs
    │   ├── IOrderRepository.cs
    │   ├── OrderRepository.cs
    │   └── ...
    │
    └── DTOs/                     # Data Transfer Objects
        ├── ProductDto.cs
        ├── OrderDto.cs
        └── ...
```

---

## 🔧 Chi tiết từng Layer

### 1️⃣ Data Access Layer (DAL)

#### **Trách nhiệm:**
- Truy cập database
- CRUD operations
- Query data
- Mapping Entity ↔ Database

#### **Thành phần:**

##### **Entities** (Domain Models)
```csharp
// DAL/Entities/Product.cs
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    // ... other properties
    
    // Navigation properties
    public virtual User Seller { get; set; }
    public virtual Category Category { get; set; }
}
```

##### **DbContext**
```csharp
// DAL/Entities/EvBatteryTrading2Context.cs
public class EvBatteryTrading2Context : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    // ... other DbSets
}
```

##### **Repositories** (Interface)
```csharp
// DAL/Repositories/IProductRepository.cs
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    // ... specific queries
}
```

##### **Repositories** (Implementation)
```csharp
// DAL/Repositories/ProductRepository.cs
public class ProductRepository : IProductRepository
{
    private readonly EvBatteryTrading2Context _context;
    
    public ProductRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }
    
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    // ... other methods
}
```

#### **✅ Repository PHẢI làm:**
- Truy cập DbContext
- Include navigation properties
- Execute queries
- SaveChangesAsync()
- Set CreatedAt, UpdatedAt

#### **❌ Repository KHÔNG ĐƯỢC làm:**
- Business logic validation
- Calculate prices, totals
- Send emails
- Call other services

---

### 2️⃣ Business Logic Layer (BLL)

#### **Trách nhiệm:**
- Business rules & validation
- Orchestrate multiple repositories
- Transform data (Entity → DTO)
- Business calculations

#### **Thành phần:**

##### **Services** (Interface)
```csharp
// BLL/Services/IProductService.cs
public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync();
    Task<(bool Success, string Message, int? ProductId)> CreateProductAsync(int sellerId, CreateProductDto dto);
    Task<(bool Success, string Message)> UpdateProductAsync(int productId, UpdateProductDto dto);
}
```

##### **Services** (Implementation)
```csharp
// BLL/Services/ProductService.cs
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    
    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }
    
    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return null;
        
        return MapToDto(product); // Transform Entity → DTO
    }
    
    public async Task<(bool Success, string Message, int? ProductId)> CreateProductAsync(
        int sellerId, CreateProductDto dto)
    {
        // ✅ Business validation
        if (dto.Price <= 0)
        {
            return (false, "Giá phải lớn hơn 0", null);
        }
        
        // ✅ Check business rules
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null)
        {
            return (false, "Danh mục không tồn tại", null);
        }
        
        // ✅ Create entity
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            SellerId = sellerId,
            CategoryId = dto.CategoryId,
            // ... map other fields
        };
        
        // ✅ Call repository
        var created = await _productRepository.CreateAsync(product);
        
        return (true, "Tạo sản phẩm thành công", created.Id);
    }
    
    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CategoryName = product.Category?.Name,
            SellerName = product.Seller?.FullName
        };
    }
}
```

#### **✅ Service PHẢI làm:**
- Business validation
- Call repositories
- Transform Entity ↔ DTO
- Orchestrate multiple repositories
- Return (Success, Message, Data) tuples

#### **❌ Service KHÔNG ĐƯỢC làm:**
- Truy cập DbContext trực tiếp
- Include() navigation properties (để Repository làm)
- SaveChangesAsync() (để Repository làm)
- UI logic (alerts, redirects)

---

### 3️⃣ Presentation Layer (UI)

#### **Trách nhiệm:**
- Display data
- Handle user input
- Call services
- Show messages

#### **Thành phần:**

##### **Razor Pages** (PageModel)
```csharp
// Pages/Products/Index.cshtml.cs
public class IndexModel : PageModel
{
    private readonly IProductService _productService;
    
    public IndexModel(IProductService productService)
    {
        _productService = productService;
    }
    
    public IEnumerable<ProductDto> Products { get; set; }
    
    public async Task OnGetAsync()
    {
        Products = await _productService.GetActiveProductsAsync();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var currentUser = HttpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }
        
        var result = await _productService.DeleteProductAsync(id, currentUser.Id);
        
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }
        
        return RedirectToPage();
    }
}
```

#### **✅ Page/Controller PHẢI làm:**
- Call services
- Handle authentication/authorization
- Set TempData messages
- Return IActionResult
- Validate user input (basic)

#### **❌ Page/Controller KHÔNG ĐƯỢC làm:**
- Call repositories directly
- Access DbContext
- Business logic
- Database queries

---

## 🔄 Data Flow Example

### Ví dụ: Tạo sản phẩm mới

```
1. User fills form → Submit
   ↓
2. Page/Controller receives request
   ├─ Validate user authentication
   ├─ Get user from session
   └─ Call ProductService.CreateProductAsync()
   ↓
3. ProductService (BLL)
   ├─ Validate business rules (price > 0, category exists)
   ├─ Check permissions
   ├─ Create Product entity
   └─ Call ProductRepository.CreateAsync()
   ↓
4. ProductRepository (DAL)
   ├─ Set CreatedAt, UpdatedAt
   ├─ _context.Products.Add(product)
   ├─ await _context.SaveChangesAsync()
   └─ Return created product
   ↓
5. ProductService returns (Success, Message, ProductId)
   ↓
6. Page/Controller
   ├─ Set TempData message
   └─ Redirect to product details
```

---

## ✅ Checklist Tuân thủ 3-Layer

### Repository Layer ✅
- [ ] Chỉ inject DbContext
- [ ] Không có business logic
- [ ] Tất cả methods đều async
- [ ] Include navigation properties khi cần
- [ ] Set timestamps (CreatedAt, UpdatedAt)
- [ ] SaveChangesAsync() sau mỗi thay đổi

### Service Layer ✅
- [ ] Inject Repositories (KHÔNG inject DbContext)
- [ ] Validate business rules
- [ ] Transform Entity ↔ DTO
- [ ] Return (Success, Message, Data) tuples
- [ ] Handle exceptions
- [ ] No direct database access

### Presentation Layer ✅
- [ ] Inject Services (KHÔNG inject Repositories/DbContext)
- [ ] Handle authentication
- [ ] Set TempData messages
- [ ] Return IActionResult
- [ ] No business logic
- [ ] No database access

---

## 🚫 Vi phạm Thường gặp

### ❌ VI PHẠM 1: Service truy cập DbContext trực tiếp
```csharp
// ❌ SAI
public class ProductService
{
    private readonly EvBatteryTrading2Context _context; // SAI!
    
    public async Task<Product> GetProductAsync(int id)
    {
        return await _context.Products.FindAsync(id); // SAI!
    }
}

// ✅ ĐÚNG
public class ProductService
{
    private readonly IProductRepository _productRepository; // ĐÚNG!
    
    public async Task<ProductDto> GetProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id); // ĐÚNG!
        return MapToDto(product);
    }
}
```

### ❌ VI PHẠM 2: Page/Controller truy cập Repository
```csharp
// ❌ SAI
public class IndexModel : PageModel
{
    private readonly IProductRepository _productRepository; // SAI!
    
    public async Task OnGetAsync()
    {
        Products = await _productRepository.GetAllAsync(); // SAI!
    }
}

// ✅ ĐÚNG
public class IndexModel : PageModel
{
    private readonly IProductService _productService; // ĐÚNG!
    
    public async Task OnGetAsync()
    {
        Products = await _productService.GetActiveProductsAsync(); // ĐÚNG!
    }
}
```

### ❌ VI PHẠM 3: Repository có business logic
```csharp
// ❌ SAI
public class ProductRepository
{
    public async Task<Product> CreateAsync(Product product)
    {
        // SAI: Business validation trong Repository
        if (product.Price <= 0)
        {
            throw new Exception("Giá phải lớn hơn 0");
        }
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }
}

// ✅ ĐÚNG
public class ProductRepository
{
    public async Task<Product> CreateAsync(Product product)
    {
        // ĐÚNG: Chỉ database operations
        product.CreatedAt = DateTime.Now;
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }
}

public class ProductService
{
    public async Task<(bool, string)> CreateProductAsync(CreateProductDto dto)
    {
        // ĐÚNG: Business validation trong Service
        if (dto.Price <= 0)
        {
            return (false, "Giá phải lớn hơn 0");
        }
        
        var product = new Product { /* ... */ };
        await _productRepository.CreateAsync(product);
        return (true, "Thành công");
    }
}
```

---

## 📝 Dependency Injection Setup

### Program.cs
```csharp
// Register DbContext
builder.Services.AddDbContext<EvBatteryTrading2Context>(options =>
    options.UseSqlServer(connectionString));

// Register Repositories (DAL)
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
// ... other repositories

// Register Services (BLL)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
// ... other services
```

---

## 🎯 Best Practices

### 1. **Always use Interfaces**
```csharp
// ✅ ĐÚNG
private readonly IProductRepository _productRepository;

// ❌ SAI
private readonly ProductRepository _productRepository;
```

### 2. **Return DTOs from Services**
```csharp
// ✅ ĐÚNG
public async Task<ProductDto> GetProductAsync(int id)

// ❌ SAI
public async Task<Product> GetProductAsync(int id)
```

### 3. **Use Tuples for Service Results**
```csharp
// ✅ ĐÚNG
public async Task<(bool Success, string Message, int? ProductId)> CreateProductAsync(...)

// ❌ SAI
public async Task<int> CreateProductAsync(...) // Không có error handling
```

### 4. **Async All The Way**
```csharp
// ✅ ĐÚNG
public async Task<Product> GetByIdAsync(int id)

// ❌ SAI
public Product GetById(int id) // Synchronous
```

---

## 📊 Summary

| Layer | Inject | Access | Responsibility |
|-------|--------|--------|----------------|
| **UI** | Services | Services only | Display, User Input |
| **BLL** | Repositories | Repositories only | Business Logic, Validation |
| **DAL** | DbContext | DbContext only | Database Access |

**Golden Rule:** Mỗi layer chỉ biết về layer ngay bên dưới nó!

---

## ✅ Đã Refactor

### Repositories Created:
- ✅ `IFavoriteRepository` & `FavoriteRepository`
- ✅ `IReviewRepository` & `ReviewRepository`
- ✅ `IMessageRepository` & `MessageRepository`
- ✅ `ISupportTicketRepository` & `SupportTicketRepository`

### Services Refactored:
- ✅ `FavoriteService` - Now uses `IFavoriteRepository`
- ✅ `ReviewService` - Now uses `IReviewRepository` & `IOrderRepository`
- ✅ `DeliveryService` - Now uses `IOrderRepository` & `IWalletRepository`

### Still Need to Refactor:
- ⏳ `ContractService` - Currently uses DbContext
- ⏳ `AuctionService` - Currently uses DbContext
- ⏳ Check all other services

---

## 🔍 How to Check Compliance

```bash
# Search for DbContext usage in Services (should be ZERO)
grep -r "EvBatteryTrading2Context" BLL/Services/

# Search for Repository usage in Pages (should be ZERO)
grep -r "Repository" PRN222_FinalProject/Pages/

# Search for DbContext usage in Pages (should be ZERO)
grep -r "DbContext" PRN222_FinalProject/Pages/
```

---

## 📚 References

- [Microsoft - 3-Tier Architecture](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/ee658109(v=pandp.10))
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
