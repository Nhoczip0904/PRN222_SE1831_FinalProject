-- =============================================
-- Script: Tạo các bảng và cột cho 7 bước nghiệp vụ logic
-- =============================================

USE [EvBatteryTrading2]
GO

-- =============================================
-- BƯỚC 5: Xác nhận nhận hàng
-- =============================================

-- Thêm cột delivery confirmation vào bảng orders
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[orders]') AND name = 'delivery_confirmed')
BEGIN
    ALTER TABLE orders ADD 
        delivery_confirmed BIT NULL,
        delivery_confirmed_at DATETIME NULL,
        delivery_notes NVARCHAR(500) NULL;
    
    PRINT 'Đã thêm cột delivery confirmation vào bảng orders';
END
GO

-- =============================================
-- BƯỚC 2: Wishlist/Favorites
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[favorites]') AND type in (N'U'))
BEGIN
    CREATE TABLE favorites (
        id INT PRIMARY KEY IDENTITY(1,1),
        user_id INT NOT NULL,
        product_id INT NOT NULL,
        created_at DATETIME DEFAULT GETDATE(),
        
        CONSTRAINT FK_favorites_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
        CONSTRAINT FK_favorites_product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
        CONSTRAINT UQ_favorites_user_product UNIQUE(user_id, product_id)
    );
    
    CREATE INDEX IX_favorites_user ON favorites(user_id);
    CREATE INDEX IX_favorites_product ON favorites(product_id);
    
    PRINT 'Đã tạo bảng favorites';
END
GO

-- =============================================
-- BƯỚC 2: Messaging/Chat
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[messages]') AND type in (N'U'))
BEGIN
    CREATE TABLE messages (
        id INT PRIMARY KEY IDENTITY(1,1),
        sender_id INT NOT NULL,
        receiver_id INT NOT NULL,
        product_id INT NULL,
        subject NVARCHAR(200) NULL,
        content NVARCHAR(2000) NOT NULL,
        is_read BIT DEFAULT 0,
        read_at DATETIME NULL,
        created_at DATETIME DEFAULT GETDATE(),
        
        CONSTRAINT FK_messages_sender FOREIGN KEY (sender_id) REFERENCES users(id),
        CONSTRAINT FK_messages_receiver FOREIGN KEY (receiver_id) REFERENCES users(id),
        CONSTRAINT FK_messages_product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE SET NULL
    );
    
    CREATE INDEX IX_messages_sender ON messages(sender_id);
    CREATE INDEX IX_messages_receiver ON messages(receiver_id);
    CREATE INDEX IX_messages_product ON messages(product_id);
    CREATE INDEX IX_messages_created ON messages(created_at DESC);
    
    PRINT 'Đã tạo bảng messages';
END
GO

-- =============================================
-- BƯỚC 6: Reviews & Ratings
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[reviews]') AND type in (N'U'))
BEGIN
    CREATE TABLE reviews (
        id INT PRIMARY KEY IDENTITY(1,1),
        order_id INT NOT NULL,
        product_id INT NOT NULL,
        buyer_id INT NOT NULL,
        seller_id INT NOT NULL,
        
        -- Đánh giá sản phẩm
        product_rating INT NOT NULL CHECK (product_rating BETWEEN 1 AND 5),
        product_review NVARCHAR(1000) NULL,
        
        -- Đánh giá người bán
        seller_rating INT NOT NULL CHECK (seller_rating BETWEEN 1 AND 5),
        seller_review NVARCHAR(1000) NULL,
        
        -- Ảnh đánh giá
        images NVARCHAR(MAX) NULL, -- JSON array
        
        -- Phản hồi từ seller
        seller_response NVARCHAR(1000) NULL,
        seller_response_at DATETIME NULL,
        
        is_verified BIT DEFAULT 1, -- Đã mua hàng thật
        is_helpful_count INT DEFAULT 0,
        
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE(),
        
        CONSTRAINT FK_reviews_order FOREIGN KEY (order_id) REFERENCES orders(id),
        CONSTRAINT FK_reviews_product FOREIGN KEY (product_id) REFERENCES products(id),
        CONSTRAINT FK_reviews_buyer FOREIGN KEY (buyer_id) REFERENCES users(id),
        CONSTRAINT FK_reviews_seller FOREIGN KEY (seller_id) REFERENCES users(id),
        CONSTRAINT UQ_reviews_order UNIQUE(order_id) -- Mỗi order chỉ review 1 lần
    );
    
    CREATE INDEX IX_reviews_product ON reviews(product_id);
    CREATE INDEX IX_reviews_seller ON reviews(seller_id);
    CREATE INDEX IX_reviews_buyer ON reviews(buyer_id);
    
    PRINT 'Đã tạo bảng reviews';
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[seller_ratings]') AND type in (N'U'))
BEGIN
    CREATE TABLE seller_ratings (
        id INT PRIMARY KEY IDENTITY(1,1),
        seller_id INT NOT NULL UNIQUE,
        total_reviews INT DEFAULT 0,
        average_rating DECIMAL(3,2) DEFAULT 0,
        five_star_count INT DEFAULT 0,
        four_star_count INT DEFAULT 0,
        three_star_count INT DEFAULT 0,
        two_star_count INT DEFAULT 0,
        one_star_count INT DEFAULT 0,
        updated_at DATETIME DEFAULT GETDATE(),
        
        CONSTRAINT FK_seller_ratings_user FOREIGN KEY (seller_id) REFERENCES users(id) ON DELETE CASCADE
    );
    
    PRINT 'Đã tạo bảng seller_ratings';
END
GO

-- =============================================
-- BƯỚC 7: Support Tickets
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[support_tickets]') AND type in (N'U'))
BEGIN
    CREATE TABLE support_tickets (
        id INT PRIMARY KEY IDENTITY(1,1),
        ticket_number VARCHAR(50) NOT NULL UNIQUE,
        user_id INT NOT NULL,
        order_id INT NULL,
        product_id INT NULL,
        
        category VARCHAR(50) NOT NULL, -- product_issue, delivery_issue, payment_issue, other
        subject NVARCHAR(200) NOT NULL,
        description NVARCHAR(2000) NOT NULL,
        images NVARCHAR(MAX) NULL, -- JSON array
        
        status VARCHAR(20) DEFAULT 'open', -- open, in_progress, resolved, closed
        priority VARCHAR(20) DEFAULT 'normal', -- low, normal, high, urgent
        
        assigned_to INT NULL, -- Admin ID
        admin_notes NVARCHAR(2000) NULL,
        resolution NVARCHAR(2000) NULL,
        resolved_at DATETIME NULL,
        
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE(),
        
        CONSTRAINT FK_tickets_user FOREIGN KEY (user_id) REFERENCES users(id),
        CONSTRAINT FK_tickets_order FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE SET NULL,
        CONSTRAINT FK_tickets_product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE SET NULL,
        CONSTRAINT FK_tickets_admin FOREIGN KEY (assigned_to) REFERENCES users(id)
    );
    
    CREATE INDEX IX_tickets_user ON support_tickets(user_id);
    CREATE INDEX IX_tickets_status ON support_tickets(status);
    CREATE INDEX IX_tickets_assigned ON support_tickets(assigned_to);
    
    PRINT 'Đã tạo bảng support_tickets';
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ticket_messages]') AND type in (N'U'))
BEGIN
    CREATE TABLE ticket_messages (
        id INT PRIMARY KEY IDENTITY(1,1),
        ticket_id INT NOT NULL,
        user_id INT NOT NULL,
        message NVARCHAR(1000) NOT NULL,
        attachments NVARCHAR(MAX) NULL,
        is_admin BIT DEFAULT 0,
        created_at DATETIME DEFAULT GETDATE(),
        
        CONSTRAINT FK_ticket_messages_ticket FOREIGN KEY (ticket_id) REFERENCES support_tickets(id) ON DELETE CASCADE,
        CONSTRAINT FK_ticket_messages_user FOREIGN KEY (user_id) REFERENCES users(id)
    );
    
    CREATE INDEX IX_ticket_messages_ticket ON ticket_messages(ticket_id);
    
    PRINT 'Đã tạo bảng ticket_messages';
END
GO

-- =============================================
-- BƯỚC 1: Advanced Search - Thêm cột vào products
-- =============================================

-- Thêm các cột nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'battery_capacity')
BEGIN
    ALTER TABLE products ADD battery_capacity DECIMAL(5,2) NULL; -- kWh
    PRINT 'Đã thêm cột battery_capacity';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'mileage')
BEGIN
    ALTER TABLE products ADD mileage INT NULL; -- km
    PRINT 'Đã thêm cột mileage';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'year_of_manufacture')
BEGIN
    ALTER TABLE products ADD year_of_manufacture INT NULL;
    PRINT 'Đã thêm cột year_of_manufacture';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'location')
BEGIN
    ALTER TABLE products ADD location NVARCHAR(200) NULL;
    PRINT 'Đã thêm cột location';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'view_count')
BEGIN
    ALTER TABLE products ADD view_count INT DEFAULT 0;
    PRINT 'Đã thêm cột view_count';
END
GO

-- =============================================
-- Tạo function tính average rating
-- =============================================

IF OBJECT_ID('dbo.fn_GetSellerAverageRating', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_GetSellerAverageRating;
GO

CREATE FUNCTION dbo.fn_GetSellerAverageRating(@seller_id INT)
RETURNS DECIMAL(3,2)
AS
BEGIN
    DECLARE @avg_rating DECIMAL(3,2);
    
    SELECT @avg_rating = AVG(CAST(seller_rating AS DECIMAL(3,2)))
    FROM reviews
    WHERE seller_id = @seller_id;
    
    RETURN ISNULL(@avg_rating, 0);
END
GO

-- =============================================
-- Tạo trigger tự động cập nhật seller_ratings
-- =============================================

IF OBJECT_ID('dbo.trg_UpdateSellerRating', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_UpdateSellerRating;
GO

CREATE TRIGGER trg_UpdateSellerRating
ON reviews
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update seller_ratings for affected sellers
    MERGE INTO seller_ratings AS target
    USING (
        SELECT 
            seller_id,
            COUNT(*) as total_reviews,
            AVG(CAST(seller_rating AS DECIMAL(3,2))) as average_rating,
            SUM(CASE WHEN seller_rating = 5 THEN 1 ELSE 0 END) as five_star_count,
            SUM(CASE WHEN seller_rating = 4 THEN 1 ELSE 0 END) as four_star_count,
            SUM(CASE WHEN seller_rating = 3 THEN 1 ELSE 0 END) as three_star_count,
            SUM(CASE WHEN seller_rating = 2 THEN 1 ELSE 0 END) as two_star_count,
            SUM(CASE WHEN seller_rating = 1 THEN 1 ELSE 0 END) as one_star_count
        FROM reviews
        WHERE seller_id IN (SELECT DISTINCT seller_id FROM inserted)
        GROUP BY seller_id
    ) AS source
    ON target.seller_id = source.seller_id
    WHEN MATCHED THEN
        UPDATE SET
            total_reviews = source.total_reviews,
            average_rating = source.average_rating,
            five_star_count = source.five_star_count,
            four_star_count = source.four_star_count,
            three_star_count = source.three_star_count,
            two_star_count = source.two_star_count,
            one_star_count = source.one_star_count,
            updated_at = GETDATE()
    WHEN NOT MATCHED THEN
        INSERT (seller_id, total_reviews, average_rating, five_star_count, four_star_count, 
                three_star_count, two_star_count, one_star_count, updated_at)
        VALUES (source.seller_id, source.total_reviews, source.average_rating, 
                source.five_star_count, source.four_star_count, source.three_star_count,
                source.two_star_count, source.one_star_count, GETDATE());
END
GO

PRINT '✅ Hoàn thành tạo tất cả bảng và cột cho 7 bước nghiệp vụ logic!';
PRINT '';
PRINT '📋 Tóm tắt:';
PRINT '- Bước 1: Thêm cột advanced search vào products';
PRINT '- Bước 2: Tạo bảng favorites và messages';
PRINT '- Bước 5: Thêm cột delivery confirmation vào orders';
PRINT '- Bước 6: Tạo bảng reviews và seller_ratings';
PRINT '- Bước 7: Tạo bảng support_tickets và ticket_messages';
GO
