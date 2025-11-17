# Update All Paging to 7 Items Per Page

## ✅ Đã update:

### 1. Products/Index.cshtml.cs
- ✅ PageSize: 12 → 7

## 📝 Cần update thủ công:

Tìm và thay đổi `PageSize` hoặc `pageSize` từ giá trị hiện tại sang **7** trong các file sau:

### 2. Auctions/Index.cshtml.cs
```csharp
// Tìm dòng có PageSize hoặc pageSize
// Thay đổi thành: PageSize = 7
```

### 3. Admin/Products/Index.cshtml.cs
```csharp
PageSize = 7
```

### 4. Admin/Orders/Index.cshtml.cs
```csharp
PageSize = 7
```

### 5. Admin/Users/Index.cshtml.cs
```csharp
PageSize = 7
```

### 6. Admin/Auctions/Index.cshtml.cs
```csharp
PageSize = 7
```

### 7. Admin/Contracts/Index.cshtml.cs
```csharp
PageSize = 7
```

## 🔍 Cách tìm nhanh:

1. **Ctrl + Shift + F** (Find in Files)
2. Search: `PageSize\s*=\s*\d+` (regex)
3. Thay tất cả thành: `PageSize = 7`

Hoặc search: `pageSize\s*=\s*\d+`

## 📊 Danh sách file cần check:

- [ ] PRN222_FinalProject/Pages/Products/Index.cshtml.cs ✅
- [ ] PRN222_FinalProject/Pages/Auctions/Index.cshtml.cs
- [ ] PRN222_FinalProject/Pages/Admin/Products/Index.cshtml.cs
- [ ] PRN222_FinalProject/Pages/Admin/Orders/Index.cshtml.cs
- [ ] PRN222_FinalProject/Pages/Admin/Users/Index.cshtml.cs
- [ ] PRN222_FinalProject/Pages/Admin/Auctions/Index.cshtml.cs
- [ ] PRN222_FinalProject/Pages/Admin/Contracts/Index.cshtml.cs
- [ ] PRN222_FinalProject/Pages/Admin/Categories/Index.cshtml.cs
- [ ] PRN222_FinalProject/Pages/Orders/Index.cshtml.cs (nếu có paging)
- [ ] PRN222_FinalProject/Pages/Transactions/Index.cshtml.cs (nếu có paging)

## 💡 Tip:

Nếu muốn thay đổi nhanh tất cả, dùng Visual Studio:
1. Edit → Find and Replace → Replace in Files
2. Find: `PageSize\s*=\s*\d+`
3. Replace: `PageSize = 7`
4. Use Regular Expressions: ✅
5. Look in: Entire Solution
6. Replace All
