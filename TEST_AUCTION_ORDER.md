# Test Auction Order Creation

## 🐛 Vấn đề hiện tại

**Triệu chứng:** Sau khi chốt đấu giá, người mua và người bán KHÔNG thấy đơn hàng trong "Đơn của tôi"

**Nguyên nhân có thể:**
1. Order không được tạo (CreateOrderFromCartAsync fail)
2. Order được tạo nhưng query không đúng
3. Order được tạo nhưng thiếu SellerId hoặc BuyerId

## 🔍 Cách debug

### **Bước 1: Rebuild và chạy**

```powershell
cd "c:\Users\Hp\Desktop\Bai Thi Pe\PRN222_FinalProject\PRN222_FinalProject"
dotnet build
dotnet run
```

### **Bước 2: Chốt đấu giá**

1. Login as seller
2. Vào trang đấu giá
3. Bấm "Chốt đấu giá"

### **Bước 3: Xem Console Output**

Bạn sẽ thấy logs chi tiết như sau:

```
[AuctionService] ===== CREATING ORDER FOR AUCTION =====
[AuctionService] AuctionId: 123
[AuctionService] WinnerId (BuyerId): 456
[AuctionService] ProductId: 789
[AuctionService] ProductName: Pin Tesla Model 3
[AuctionService] Price: 10000000
[AuctionService] Quantity: 1
[AuctionService] TotalPrice: 10000000
[AuctionService] SellerId: 999
[AuctionService] PaymentMethod: auction

[OrderService] CreateOrderFromCartAsync called - BuyerId: 456, PaymentMethod: auction
[OrderService] Total amount: 10000000, Items count: 1
[OrderService] Is auction order: True
[OrderService] Creating contract for order #1001
[OrderService] Contract created successfully for order #1001
[OrderService] Order #1001 added to result list
[OrderService] SUCCESS: Created 1 order(s). First OrderId: 1001

[AuctionService] Order creation result: Success=True, Message=..., OrderId=1001
[AuctionService] Auto-confirming order #1001
```

### **Bước 4: Check Database**

```sql
-- Check order
SELECT * FROM orders 
WHERE id = 1001 OR payment_method = 'auction'
ORDER BY created_at DESC;

-- Kiểm tra buyer_id và seller_id
SELECT 
    o.id,
    o.buyer_id,
    o.seller_id,
    o.status,
    o.payment_method,
    o.total_amount,
    o.created_at
FROM orders o
WHERE o.payment_method = 'auction'
ORDER BY o.created_at DESC;

-- Check contract
SELECT * FROM contracts 
WHERE order_id = 1001;

-- Check order items
SELECT * FROM order_items 
WHERE order_id = 1001;
```

### **Bước 5: Check UI Query**

Kiểm tra xem trang "Đơn của tôi" query như thế nào:

```sql
-- Query cho người mua
SELECT * FROM orders 
WHERE buyer_id = 456  -- WinnerId
ORDER BY created_at DESC;

-- Query cho người bán
SELECT * FROM orders 
WHERE seller_id = 999  -- SellerId
ORDER BY created_at DESC;
```

## ❌ Các lỗi thường gặp

### **Lỗi 1: "Sản phẩm không còn khả dụng"**

**Console log:**
```
[OrderService] ERROR: Sản phẩm 'Pin Tesla' không còn khả dụng hoặc đã được bán
```

**Nguyên nhân:** Product validation fail (đã fix với `if (!isAuctionOrder)`)

**Giải pháp:** ✅ Đã fix trong OrderService.cs line 103

### **Lỗi 2: "Giỏ hàng trống"**

**Console log:**
```
[OrderService] ERROR: Cart is empty
```

**Nguyên nhân:** CartItems null hoặc empty

**Giải pháp:** Check AuctionService có tạo cartItems đúng không

### **Lỗi 3: Order created nhưng OrderId = null**

**Console log:**
```
[AuctionService] Order creation result: Success=True, Message=..., OrderId=null
[AuctionService] WARNING: Order created but OrderId is null!
```

**Nguyên nhân:** `createdOrderIds.FirstOrDefault()` return null

**Giải pháp:** Check xem order có được add vào list không

### **Lỗi 4: Order created nhưng không thấy trong UI**

**Console log:**
```
[OrderService] SUCCESS: Created 1 order(s). First OrderId: 1001
```

**Database:**
```sql
SELECT * FROM orders WHERE id = 1001;
-- Có data
```

**UI:** Không thấy order

**Nguyên nhân:** Query trong UI không đúng hoặc thiếu buyer_id/seller_id

**Debug:**
1. Check `buyer_id` trong database:
   ```sql
   SELECT buyer_id, seller_id FROM orders WHERE id = 1001;
   ```
2. Check query trong Orders/Index.cshtml.cs:
   ```csharp
   // Người mua
   var buyerOrders = await _orderService.GetOrdersByBuyerIdAsync(userId);
   
   // Người bán
   var sellerOrders = await _orderService.GetOrdersBySellerIdAsync(userId);
   ```

## 🔧 Fix nếu thiếu buyer_id hoặc seller_id

Nếu database show:
```
id  | buyer_id | seller_id | status
1001| 456      | NULL      | confirmed
```

**Vấn đề:** `seller_id` bị NULL!

**Nguyên nhân:** OrderService không set seller_id đúng

**Fix:** Check OrderService.cs line ~130-150

```csharp
var order = new Order
{
    BuyerId = buyerId,
    SellerId = sellerId,  // ← Phải có dòng này
    TotalAmount = orderTotal,
    Status = "pending",
    // ...
};
```

## 📊 Expected Results

### **Console Output (Success):**
```
[AuctionService] ===== CREATING ORDER FOR AUCTION =====
[AuctionService] WinnerId (BuyerId): 456
[AuctionService] SellerId: 999
[OrderService] SUCCESS: Created 1 order(s). First OrderId: 1001
[AuctionService] Auto-confirming order #1001
```

### **Database:**
```
orders table:
id  | buyer_id | seller_id | status    | payment_method | total_amount
1001| 456      | 999       | confirmed | auction        | 10000000

contracts table:
id | order_id | buyer_id | seller_id | status
1  | 1001     | 456      | 999       | pending
```

### **UI:**
- **Người mua (userId=456):** Vào "Đơn của tôi" → Tab "Đơn mua" → Thấy order #1001
- **Người bán (userId=999):** Vào "Đơn của tôi" → Tab "Đơn bán" → Thấy order #1001

## 🚀 Quick Test Script

```sql
-- 1. Check auction
SELECT * FROM auctions WHERE id = 123;

-- 2. Check bids
SELECT * FROM bids WHERE auction_id = 123 ORDER BY bid_amount DESC;

-- 3. Check order created
SELECT * FROM orders WHERE payment_method = 'auction' ORDER BY created_at DESC LIMIT 5;

-- 4. Check buyer can see order
SELECT * FROM orders WHERE buyer_id = 456;

-- 5. Check seller can see order
SELECT * FROM orders WHERE seller_id = 999;

-- 6. Check contract
SELECT c.*, o.total_amount 
FROM contracts c
INNER JOIN orders o ON c.order_id = o.id
WHERE o.payment_method = 'auction';
```

## 📝 Checklist

Sau khi chốt đấu giá, check:

- [ ] Console log: `[AuctionService] ===== CREATING ORDER FOR AUCTION =====`
- [ ] Console log: `[OrderService] Is auction order: True`
- [ ] Console log: `[OrderService] SUCCESS: Created 1 order(s)`
- [ ] Console log: `[AuctionService] Auto-confirming order #...`
- [ ] Database: `SELECT * FROM orders WHERE payment_method = 'auction'` → Có data
- [ ] Database: `buyer_id` NOT NULL
- [ ] Database: `seller_id` NOT NULL
- [ ] Database: `status = 'confirmed'`
- [ ] UI (Buyer): Thấy order trong "Đơn mua"
- [ ] UI (Seller): Thấy order trong "Đơn bán"
- [ ] UI (Both): Thấy contract

## 📞 Nếu vẫn lỗi

Gửi cho tôi:
1. **Console logs** (toàn bộ từ khi bấm "Chốt đấu giá")
2. **SQL query results:**
   ```sql
   SELECT * FROM orders WHERE payment_method = 'auction' ORDER BY created_at DESC LIMIT 1;
   ```
3. **Screenshot** trang "Đơn của tôi" (cả buyer và seller)

Tôi sẽ debug tiếp!
