# Staff Role Testing Guide

## Test Cases for Staff Role Implementation

### 1. Create Staff User
1. Login as Admin
2. Navigate to Admin/Users/Create
3. Fill in user details:
   - Email: staff@example.com
   - Password: Staff123!
   - Full Name: Test Staff
   - Phone: 0123456789
   - Role: Staff (select from dropdown)
   - Check "Xác thực tài khoản ngay"
4. Click "Thêm người dùng"
5. Verify success message and user appears in list with yellow "Staff" badge

### 2. Test Staff Login and Navigation
1. Logout from Admin account
2. Login with staff credentials (staff@example.com / Staff123!)
3. Verify:
   - Page title shows "Management Panel"
   - Navigation menu shows:
     - Quản lý người dùng (Users)
     - Duyệt hợp đồng (Contracts)
     - Quản lý đơn hàng (Orders)
     - Quản lý danh mục (Categories)
     - Duyệt sản phẩm (Product Approval)
     - Quản lý đấu giá (Auction Management)
   - Should NOT see:
     - Dashboard (Admin only)

### 3. Test Staff Permissions
1. As Staff user, try to access restricted pages:
   - Try accessing `/Admin/Dashboard` directly - should redirect to login (Admin only)

2. Verify Staff CAN access:
   - `/Admin/Users/Index` - Should show users list
   - `/Admin/Users/Create` - Should show create user form
   - `/Admin/Users/Edit/{id}` - Should show edit user form
   - `/Admin/Contracts/Index` - Should show pending contracts
   - `/Admin/Orders/Index` - Should show orders list
   - `/Admin/Categories/Index` - Should show categories list
   - `/Admin/Categories/Create` - Should show create category form
   - `/Admin/Categories/Edit/{id}` - Should show edit category form
   - `/Staff/Products/Pending` - Should show pending products page
   - `/Staff/Auctions/Index` - Should show auctions management page

### 4. Test User Management
1. As Staff user, go to Users management
2. Should be able to:
   - View all users
   - Create new users
   - Edit existing users
   - Suspend/activate users (except admins)
   - Delete users (except admins and other staff)

### 5. Test Contract Management
1. As Staff user, go to Contract Approval page
2. Should see list of pending contracts
3. Can approve/reject contracts

### 6. Test Order Management
1. As Staff user, go to Order Management page
2. Should see list of all orders
3. Can update order status

### 7. Test Category Management
1. As Staff user, go to Category Management page
2. Should see list of categories
3. Can create new categories
4. Can edit existing categories
5. Can delete categories (with no products)

### 8. Test Product Approval
1. As Staff user, go to Product Approval page
2. Should see list of pending products
3. Can approve/reject products
4. Should receive real-time notifications for new pending products (SignalR)

### 9. Test Auction Management
1. As Staff user, go to Auction Management page
2. Should see list of active auctions
3. Can close auctions

### 10. Test Admin Role (Dashboard Only)
1. Login as Admin
2. Verify:
   - Page title shows "Management Panel"
   - Full navigation menu is visible including Dashboard
   - Can access all admin pages
   - Dashboard is accessible

### 11. Test User Management Restrictions
1. As Admin or Staff, view Users list
2. Verify:
   - Staff users have yellow "Staff" badge
   - Staff users cannot be suspended/deleted by other staff
   - Only Admins can suspend/delete other staff users
   - Admins cannot be suspended/deleted by anyone

### 12. Test SignalR Notifications
1. As Staff user, keep Product Approval page open
2. As Seller user (in another browser), create a new product
3. Staff should receive notification about new pending product
4. Product list should refresh automatically after 2 seconds

## Expected Results Summary
- ✅ Staff users have access to all management functions except Dashboard
- ✅ Admin users retain full access including Dashboard
- ✅ Navigation menu dynamically adjusts based on user role
- ✅ Staff users receive real-time notifications for new products
- ✅ User management restrictions work correctly
- ✅ All authorization checks work correctly
