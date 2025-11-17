# Debug: Đấu giá → Đơn hàng → Hợp đồng

## 🔧 Các fix đã thực hiện

### **1. Skip product validation cho auction orders**

**Vấn đề:** Sản phẩm đấu giá có thể đã bị đánh dấu `IsSold = true` hoặc `IsActive = false`, khiến validation fail.

**Fix:** Skip validation check cho auction orders

```csharp
// OrderService.cs line 103-118
if (!isAuctionOrder)
{
    foreach (var item in items)
    {
        var product = await _productRepository.GetByIdAsync(item.ProductId);
        if (product == null || product.IsActive == false || product.IsSold == true)
        {
            return (false, $"Sản phẩm '{item.ProductName}' không còn khả dụng", null);
        }
    }
}
```

### **2. Thêm logging chi tiết**

**Mục đích:** Debug từng bước để tìm lỗi

**Logs được thêm:**
- `[AuctionService]` - Khi chốt đấu giá
- `[OrderService]` - Khi tạo order
- `[OrderService]` - Khi tạo contract
- `[OrderService]` - Khi thành công

## 📊 Cách debug

### **Bước 1: Chạy ứng dụng**

```bash
dotnet run
```

### **Bước 2: Chốt đấu giá**

1. Login as seller
2. Vào trang đấu giá
3. Bấm "Chốt đấu giá"

### **Bước 3: Xem Console Output**

Bạn sẽ thấy logs như sau:

```
[AuctionService] Creating order for auction #123, winner: 456
[AuctionService] Cart items: ProductId=789, Price=1000000, Quantity=1

[OrderService] CreateOrderFromCartAsync called - BuyerId: 456, PaymentMethod: auction
[OrderService] Total amount: 1000000, Items count: 1
[OrderService] Is auction order: True
[OrderService] Creating contract for order #999
[OrderService] Contract created successfully for order #999
[OrderService] Order #999 added to result list
[OrderService] SUCCESS: Created 1 order(s). First OrderId: 999

[AuctionService] Order creation result: Success=True, Message=..., OrderId=999
[AuctionService] Auto-confirming order #999
```

### **Bước 4: Kiểm tra Database**

```sql
-- Check order
SELECT * FROM orders WHERE id = 999;

-- Check contract
SELECT * FROM contracts WHERE order_id = 999;

-- Check order items
SELECT * FROM order_items WHERE order_id = 999;
```

## ❌ Các lỗi có thể gặp

### **Lỗi 1: "Sản phẩm không còn khả dụng"**

**Nguyên nhân:** Product validation fail

**Giải pháp:** ✅ Đã fix - Skip validation cho auction

### **Lỗi 2: "Order created but OrderId is null"**

**Nguyên nhân:** `CreateOrderFromCartAsync` return null OrderId

**Debug:**
```
[OrderService] SUCCESS: Created 1 order(s). First OrderId: null
```

**Giải pháp:** Check xem order có được add vào `createdOrderIds` không

### **Lỗi 3: "Contract creation failed"**

**Nguyên nhân:** ContractService throw exception

**Debug:**
```
[OrderService] ERROR creating contract: ...
[OrderService] Stack trace: ...
```

**Giải pháp:** Xem stack trace để biết lỗi gì

### **Lỗi 4: Không thấy order trong UI**

**Nguyên nhân:** Order được tạo nhưng query không đúng

**Debug:**
1. Check database: `SELECT * FROM orders WHERE buyer_id = 456`
2. Nếu có order → Vấn đề ở UI/query
3. Nếu không có order → Vấn đề ở create logic

## 🔍 Checklist Debug

### **Sau khi chốt đấu giá:**

- [ ] Console log: `[AuctionService] Creating order`
- [ ] Console log: `[OrderService] CreateOrderFromCartAsync called`
- [ ] Console log: `[OrderService] Is auction order: True`
- [ ] Console log: `[OrderService] Creating contract`
- [ ] Console log: `[OrderService] Contract created successfully`
- [ ] Console log: `[OrderService] SUCCESS: Created 1 order(s)`
- [ ] Console log: `[AuctionService] Auto-confirming order`
- [ ] Database: Order exists in `orders` table
- [ ] Database: Contract exists in `contracts` table
- [ ] UI: Order hiển thị trong "Đơn hàng của tôi"
- [ ] UI: Contract hiển thị trong "Hợp đồng"

## 🎯 Test Case

### **Test 1: Chốt đấu giá thành công**

**Steps:**
1. Tạo auction
2. User A đặt giá
3. Seller chốt đấu giá
4. Check console logs
5. Check database
6. Check UI (buyer & seller)

**Expected:**
- ✅ Order created (status = "confirmed")
- ✅ Contract created (status = "pending")
- ✅ Buyer thấy order trong "Đơn mua"
- ✅ Seller thấy order trong "Đơn bán"
- ✅ Cả 2 thấy contract

### **Test 2: Nhiều người đặt giá**

**Steps:**
1. User A đặt giá 1,000,000
2. User B đặt giá 1,500,000
3. User C đặt giá 1,200,000
4. Seller chốt đấu giá (winner = User B)

**Expected:**
- ✅ Order created cho User B (người giá cao nhất)
- ✅ User A và C không có order

## 📝 SQL Queries để debug

### **Check order của buyer:**
```sql
SELECT o.*, oi.* 
FROM orders o
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.buyer_id = @buyerId
ORDER BY o.created_at DESC;
```

### **Check order của seller:**
```sql
SELECT o.*, oi.* 
FROM orders o
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.seller_id = @sellerId
ORDER BY o.created_at DESC;
```

### **Check contract:**
```sql
SELECT c.*, o.total_amount, o.status as order_status
FROM contracts c
INNER JOIN orders o ON c.order_id = o.id
WHERE c.buyer_id = @userId OR c.seller_id = @userId
ORDER BY c.created_at DESC;
```

### **Check auction và order:**
```sql
SELECT 
    a.id as auction_id,
    a.status as auction_status,
    a.winner_id,
    o.id as order_id,
    o.status as order_status,
    o.total_amount
FROM auctions a
LEFT JOIN orders o ON o.payment_method = 'auction' 
    AND o.buyer_id = a.winner_id
WHERE a.id = @auctionId;
```

## 🚀 Sau khi fix

1. **Rebuild solution**
   ```bash
   dotnet build
   ```

2. **Chạy lại ứng dụng**
   ```bash
   dotnet run
   ```

3. **Test lại luồng đấu giá**

4. **Xem console logs**

5. **Check database**

6. **Verify UI**

## 📞 Nếu vẫn lỗi

Gửi cho tôi:
1. Console logs (toàn bộ)
2. Screenshot lỗi (nếu có)
3. SQL query results
4. Screenshot UI (trang đơn hàng)

Tôi sẽ debug tiếp!
