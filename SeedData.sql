-- Seed Data for EV Battery Trading Platform
-- Chủ đề 1: Quản lý Tài khoản

USE ev_battery_trading2;
GO

-- Clear existing data (optional - comment out if you want to keep existing data)
-- DELETE FROM order_items;
-- DELETE FROM transactions;
-- DELETE FROM orders;
-- DELETE FROM products;
-- DELETE FROM users;
-- DELETE FROM categories;

-- Insert Admin User
-- Password: Admin@123 (hashed with BCrypt)
INSERT INTO users (email, phone, full_name, address, password_hash, role, is_verified, created_at, updated_at)
VALUES 
('admin@evbattery.com', '0901234567', 'Admin System', 'Hà Nội, Việt Nam', 
 '$2a$11$rGH2t0o/k0f5axQ.slwCN.btbJTwWggyzAd5W5tEHVpLolIE34jGi', -- Admin@123
 'admin', 1, GETDATE(), GETDATE());

-- Insert Sample Members
-- Password: User@123 (hashed with BCrypt)
INSERT INTO users (email, phone, full_name, address, password_hash, role, is_verified, created_at, updated_at)
VALUES 
('user1@evbattery.com', '0912345678', 'Nguyễn Văn A', '123 Đường ABC, Quận 1, TP.HCM', 
 '$2a$11$.v14Jeg7cjltuc58wziiCeVLTGa8H8o1LvTndhAqGdB8EKRdkWnE.', -- User@123
 'member', 1, GETDATE(), GETDATE()),
 
('user2@evbattery.com', '0923456789', 'Trần Thị B', '456 Đường XYZ, Quận 2, TP.HCM', 
 '$2a$11$.v14Jeg7cjltuc58wziiCeVLTGa8H8o1LvTndhAqGdB8EKRdkWnE.', -- User@123
 'member', 1, GETDATE(), GETDATE()),
 
('user3@evbattery.com', '0934567890', 'Lê Văn C', '789 Đường DEF, Quận 3, TP.HCM', 
 '$2a$11$.v14Jeg7cjltuc58wziiCeVLTGa8H8o1LvTndhAqGdB8EKRdkWnE.', -- User@123
 'member', 0, GETDATE(), GETDATE()), -- Unverified user
 
('seller1@evbattery.com', '0945678901', 'Phạm Thị D', '321 Đường GHI, Quận 4, TP.HCM', 
 '$2a$11$.v14Jeg7cjltuc58wziiCeVLTGa8H8o1LvTndhAqGdB8EKRdkWnE.', -- User@123
 'member', 1, GETDATE(), GETDATE()),
 
('buyer1@evbattery.com', '0956789012', 'Hoàng Văn E', '654 Đường JKL, Quận 5, TP.HCM', 
 '$2a$11$.v14Jeg7cjltuc58wziiCeVLTGa8H8o1LvTndhAqGdB8EKRdkWnE.', -- User@123
 'member', 1, GETDATE(), GETDATE());

-- Insert Categories
INSERT INTO categories (name, description, created_at, updated_at)
VALUES 
('Tesla Battery', 'Pin dành cho xe Tesla các loại', GETDATE(), GETDATE()),
('VinFast Battery', 'Pin dành cho xe VinFast', GETDATE(), GETDATE()),
('BMW Battery', 'Pin dành cho xe BMW điện', GETDATE(), GETDATE()),
('Generic EV Battery', 'Pin chung cho xe điện', GETDATE(), GETDATE());

PRINT 'Seed data completed successfully!';
PRINT '';
PRINT 'Sample Accounts:';
PRINT '================';
PRINT 'Admin Account:';
PRINT '  Email: admin@evbattery.com';
PRINT '  Password: Admin@123';
PRINT '';
PRINT 'Member Accounts:';
PRINT '  Email: user1@evbattery.com, Password: User@123 (Verified)';
PRINT '  Email: user2@evbattery.com, Password: User@123 (Verified)';
PRINT '  Email: user3@evbattery.com, Password: User@123 (NOT Verified)';
PRINT '  Email: seller1@evbattery.com, Password: User@123 (Verified)';
PRINT '  Email: buyer1@evbattery.com, Password: User@123 (Verified)';
PRINT '';
PRINT 'Note: Password hashes shown above are placeholders.';
PRINT 'You need to register new accounts or use BCrypt to generate correct hashes.';
GO
