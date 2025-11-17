# 🔄 Unit of Work Pattern - Hướng dẫn Sử dụng

## 📋 Tổng quan

**Unit of Work** là một design pattern quản lý transactions và đảm bảo tính nhất quán của dữ liệu.

### ✅ Lợi ích:
- **Transaction Management**: Quản lý transactions dễ dàng
- **Single SaveChanges**: Chỉ gọi SaveChanges một lần cho nhiều operations
- **Consistency**: Đảm bảo tính nhất quán dữ liệu
- **Cleaner Code**: Code sạch hơn, dễ maintain

---

## 🏗️ Cấu trúc

```
DAL/Repositories/
├── IUnitOfWork.cs          # Interface
└── UnitOfWork.cs           # Implementation
```

### Interface
```csharp
public interface IUnitOfWork : IDisposable
{
    // Repositories
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    // ... other repositories
    
    // Methods
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

---

## 💡 Cách Sử dụng

### 1. Inject IUnitOfWork vào Service

```csharp
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
```

### 2. Sử dụng Repositories qua UnitOfWork

```csharp
public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
{
    // Access repositories through UnitOfWork
    var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
    var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
    
    var order = new Order { /* ... */ };
    await _unitOfWork.Orders.CreateAsync(order);
    
    // Single SaveChanges for all operations
    await _unitOfWork.SaveChangesAsync();
    
    return order;
}
```

### 3. Sử dụng Transactions

```csharp
public async Task<(bool Success, string Message)> ProcessOrderAsync(int orderId)
{
    try
    {
        // Begin transaction
        await _unitOfWork.BeginTransactionAsync();
        
        // Multiple operations
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        order.Status = "processing";
        
        var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(order.BuyerId);
        wallet.Balance -= order.TotalAmount;
        
        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.Wallets.UpdateAsync(wallet);
        
        // Commit all changes
        await _unitOfWork.CommitTransactionAsync();
        
        return (true, "Success");
    }
    catch (Exception ex)
    {
        // Rollback on error
        await _unitOfWork.RollbackTransactionAsync();
        return (false, ex.Message);
    }
}
```

---

## 📝 Ví dụ Thực tế

### Example 1: Create Order with Contract

```csharp
public async Task<(bool Success, string Message)> CreateOrderWithContractAsync(CreateOrderDto dto)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();
        
        // 1. Create Order
        var order = new Order
        {
            BuyerId = dto.BuyerId,
            SellerId = dto.SellerId,
            TotalAmount = dto.TotalAmount,
            Status = "pending"
        };
        await _unitOfWork.Orders.CreateAsync(order);
        
        // 2. Create Contract
        var contract = new Contract
        {
            OrderId = order.Id,
            BuyerId = dto.BuyerId,
            SellerId = dto.SellerId,
            ContractNumber = $"HD{DateTime.Now:yyyyMMdd}{order.Id:D6}",
            TotalAmount = dto.TotalAmount,
            Status = "pending"
        };
        await _unitOfWork.Contracts.CreateAsync(contract);
        
        // 3. Commit transaction
        await _unitOfWork.CommitTransactionAsync();
        
        return (true, "Order and contract created successfully");
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        return (false, $"Error: {ex.Message}");
    }
}
```

### Example 2: Confirm Delivery & Release Funds

```csharp
public async Task<(bool Success, string Message)> ConfirmDeliveryAsync(int orderId, int buyerId)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();
        
        // 1. Update Order
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        order.Status = "delivered";
        order.DeliveryConfirmed = true;
        order.DeliveryConfirmedAt = DateTime.Now;
        await _unitOfWork.Orders.UpdateAsync(order);
        
        // 2. Transfer Money to Seller
        var sellerWallet = await _unitOfWork.Wallets.GetByUserIdAsync(order.SellerId);
        sellerWallet.Balance += order.TotalAmount;
        await _unitOfWork.Wallets.UpdateAsync(sellerWallet);
        
        // 3. Create Transaction Record
        var transaction = new WalletTransaction
        {
            WalletId = sellerWallet.Id,
            Type = "credit",
            Amount = order.TotalAmount,
            Description = $"Payment from order #{orderId}",
            Status = "completed"
        };
        await _unitOfWork.WalletTransactions.CreateAsync(transaction);
        
        // 4. Commit all changes
        await _unitOfWork.CommitTransactionAsync();
        
        return (true, "Delivery confirmed and funds released");
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        return (false, $"Error: {ex.Message}");
    }
}
```

---

## ⚖️ So sánh: Với & Không có UnitOfWork

### ❌ KHÔNG có UnitOfWork (Cũ)

```csharp
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IWalletRepository _walletRepository;
    
    public OrderService(
        IOrderRepository orderRepository,
        IContractRepository contractRepository,
        IWalletRepository walletRepository)
    {
        _orderRepository = orderRepository;
        _contractRepository = contractRepository;
        _walletRepository = walletRepository;
    }
    
    public async Task CreateOrderAsync(...)
    {
        var order = new Order { /* ... */ };
        await _orderRepository.CreateAsync(order); // SaveChanges #1
        
        var contract = new Contract { /* ... */ };
        await _contractRepository.CreateAsync(contract); // SaveChanges #2
        
        // Multiple SaveChanges calls!
    }
}
```

### ✅ CÓ UnitOfWork (Mới)

```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task CreateOrderAsync(...)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        var order = new Order { /* ... */ };
        await _unitOfWork.Orders.CreateAsync(order);
        
        var contract = new Contract { /* ... */ };
        await _unitOfWork.Contracts.CreateAsync(contract);
        
        await _unitOfWork.CommitTransactionAsync(); // Single SaveChanges!
    }
}
```

---

## 🎯 Best Practices

### 1. Always Use Try-Catch with Transactions

```csharp
try
{
    await _unitOfWork.BeginTransactionAsync();
    // ... operations
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

### 2. Dispose UnitOfWork Properly

```csharp
// UnitOfWork is Scoped - auto-disposed by DI container
// But if you create manually:
using (var unitOfWork = new UnitOfWork(context))
{
    // ... operations
}
```

### 3. Don't Mix Direct Repository Injection with UnitOfWork

```csharp
// ❌ BAD - Mixing approaches
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository; // Don't do this!
}

// ✅ GOOD - Use UnitOfWork only
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    // Access via: _unitOfWork.Orders
}
```

---

## 📊 Khi nào dùng UnitOfWork?

### ✅ Nên dùng khi:
- Cần transaction cho nhiều operations
- Cần đảm bảo tính nhất quán dữ liệu
- Có nhiều repositories trong một service
- Cần rollback khi có lỗi

### ⚠️ Có thể không cần khi:
- Chỉ có 1 operation đơn giản
- Không cần transaction
- Service chỉ dùng 1 repository

---

## 🔧 Troubleshooting

### Error: "Cannot access disposed object"
**Nguyên nhân:** UnitOfWork đã bị dispose
**Giải pháp:** Đảm bảo UnitOfWork là Scoped trong DI

### Error: "Transaction already started"
**Nguyên nhân:** Gọi BeginTransaction nhiều lần
**Giải pháp:** Chỉ gọi BeginTransaction một lần

### Error: "SaveChanges called multiple times"
**Nguyên nhân:** Repository tự gọi SaveChanges
**Giải pháp:** Xóa SaveChanges trong Repository, chỉ gọi qua UnitOfWork

---

## ✅ Checklist Triển khai

- [x] Tạo IUnitOfWork interface
- [x] Tạo UnitOfWork implementation
- [x] Đăng ký trong Program.cs
- [x] Update Services để dùng IUnitOfWork
- [ ] Test transactions
- [ ] Test rollback
- [ ] Update documentation

---

## 📚 Tài liệu Tham khảo

- [Martin Fowler - Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [Microsoft - Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

---

**Hệ thống giờ đây đã có Unit of Work pattern hoàn chỉnh! 🎉**
