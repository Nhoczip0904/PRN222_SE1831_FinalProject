# 🔄 CHỨC NĂNG SO SÁNH SẢN PHẨM

## 🎯 Mục đích
Giúp người dùng so sánh tối đa 4 sản phẩm cùng lúc để đưa ra quyết định mua hàng tốt nhất.

---

## 📋 Tính năng

### 1. Thêm sản phẩm vào danh sách so sánh
- Từ trang danh sách sản phẩm
- Click nút "So sánh" trên mỗi sản phẩm
- Tối đa 4 sản phẩm

### 2. Xem bảng so sánh
- Hiển thị các tiêu chí:
  - ✅ Hình ảnh
  - ✅ Tên sản phẩm
  - ✅ Giá (highlight giá rẻ nhất)
  - ✅ Sức khỏe pin (highlight cao nhất)
  - ✅ Tình trạng
  - ✅ Danh mục
  - ✅ Người bán
  - ✅ Ngày đăng
  - ✅ Mô tả

### 3. Highlight tự động
- 🏆 Giá rẻ nhất: Màu xanh + icon trophy
- 🏆 Pin tốt nhất: Màu xanh + icon trophy

### 4. Xóa sản phẩm khỏi so sánh
- Click nút X trên mỗi cột

---

## 🗂️ Files đã tạo

### 1. DTO
```
DAL/DTOs/CompareProductDto.cs
```

### 2. Pages
```
Pages/Products/Compare.cshtml
Pages/Products/Compare.cshtml.cs
```

### 3. Cập nhật
```
Pages/Products/Index.cshtml - Thêm nút so sánh
Pages/Products/Index.cshtml.cs - Thêm handler
```

---

## 💻 Cách sử dụng

### Bước 1: Thêm sản phẩm
```
1. Vào trang "Sản phẩm"
2. Tìm sản phẩm muốn so sánh
3. Click "So sánh" (tối đa 4 sản phẩm)
```

### Bước 2: Xem so sánh
```
1. Click nút "So sánh (X)" ở góc phải
2. Xem bảng so sánh chi tiết
3. Giá rẻ nhất và pin tốt nhất được highlight
```

### Bước 3: Quyết định
```
1. So sánh các tiêu chí
2. Click "Xem chi tiết" để xem thêm
3. Quyết định mua sản phẩm phù hợp
```

---

## 🎨 Giao diện

### Trang danh sách sản phẩm
```
┌─────────────────────────────────────────┐
│ Danh sách sản phẩm    [So sánh (2)]    │
├─────────────────────────────────────────┤
│ [Sản phẩm A]                            │
│ [Xem chi tiết] [So sánh]                │
│                                         │
│ [Sản phẩm B]                            │
│ [Xem chi tiết] [So sánh]                │
└─────────────────────────────────────────┘
```

### Trang so sánh
```
┌────────────────────────────────────────────────────┐
│ So sánh sản phẩm              [Quay lại]          │
├────────────────────────────────────────────────────┤
│ Tiêu chí    │ Sản phẩm A [X] │ Sản phẩm B [X]    │
├────────────────────────────────────────────────────┤
│ Hình ảnh    │ [IMG]          │ [IMG]             │
│ Giá         │ 10M 🏆         │ 12M               │
│ Pin         │ 80%            │ 90% 🏆            │
│ Tình trạng  │ Mới            │ Đã qua sử dụng    │
└────────────────────────────────────────────────────┘
```

---

## 🔧 Technical Details

### Session Storage
```csharp
// Lưu danh sách ID sản phẩm trong session
Session["CompareProducts"] = [1, 2, 3, 4]
```

### Highlight Logic (JavaScript)
```javascript
// Tìm giá thấp nhất
function highlightBestPrice() {
    // Tìm cell có giá thấp nhất
    // Thêm background màu xanh
    // Thêm icon trophy
}

// Tìm pin tốt nhất
function highlightBestBattery() {
    // Tìm cell có pin cao nhất
    // Thêm background màu xanh
    // Thêm icon trophy
}
```

---

## ✅ Đã triển khai

- ✅ DTO cho so sánh
- ✅ Trang Compare.cshtml
- ✅ PageModel Compare.cshtml.cs
- ✅ Nút "So sánh" trong Index
- ✅ Handler AddToCompare
- ✅ Session extensions
- ✅ Highlight tự động
- ✅ Responsive design

---

## 🧪 Test Cases

### Test 1: Thêm sản phẩm
```
1. Click "So sánh" trên sản phẩm A
2. Kiểm tra: Số đếm tăng lên (1)
3. Click "So sánh" trên sản phẩm B
4. Kiểm tra: Số đếm tăng lên (2)
```

### Test 2: Giới hạn 4 sản phẩm
```
1. Thêm 4 sản phẩm
2. Thử thêm sản phẩm thứ 5
3. Kiểm tra: Hiển thị lỗi "Chỉ có thể so sánh tối đa 4 sản phẩm"
```

### Test 3: Xem so sánh
```
1. Thêm 3 sản phẩm
2. Click "So sánh (3)"
3. Kiểm tra: Hiển thị bảng 3 cột
4. Kiểm tra: Giá rẻ nhất được highlight
5. Kiểm tra: Pin tốt nhất được highlight
```

### Test 4: Xóa sản phẩm
```
1. Trong trang so sánh
2. Click [X] trên sản phẩm B
3. Kiểm tra: Sản phẩm B biến mất
4. Kiểm tra: Còn 2 sản phẩm
```

---

## 🚀 Hoàn thành 100%

Chức năng so sánh sản phẩm đã sẵn sàng sử dụng!
