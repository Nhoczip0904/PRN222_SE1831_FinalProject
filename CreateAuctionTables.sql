-- Create Auction Tables for EV Battery Trading Platform
USE ev_battery_trading2;
GO

-- Create Auctions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'auctions')
BEGIN
    CREATE TABLE auctions (
        id INT IDENTITY(1,1) PRIMARY KEY,
        product_id INT NOT NULL,
        seller_id INT NOT NULL,
        starting_price DECIMAL(18,2) NOT NULL CHECK (starting_price > 0),
        current_price DECIMAL(18,2),
        reserve_price DECIMAL(18,2),
        start_time DATETIME NOT NULL,
        end_time DATETIME NOT NULL,
        status VARCHAR(20) NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'closed', 'cancelled')),
        winner_id INT NULL,
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_auctions_products FOREIGN KEY (product_id) REFERENCES products(id),
        CONSTRAINT FK_auctions_seller FOREIGN KEY (seller_id) REFERENCES users(id),
        CONSTRAINT FK_auctions_winner FOREIGN KEY (winner_id) REFERENCES users(id),
        CONSTRAINT CHK_auction_times CHECK (end_time > start_time)
    );
    
    PRINT 'Table auctions created successfully';
END
ELSE
BEGIN
    PRINT 'Table auctions already exists';
END
GO

-- Create Bids table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'bids')
BEGIN
    CREATE TABLE bids (
        id INT IDENTITY(1,1) PRIMARY KEY,
        auction_id INT NOT NULL,
        bidder_id INT NOT NULL,
        bid_amount DECIMAL(18,2) NOT NULL CHECK (bid_amount > 0),
        bid_time DATETIME NOT NULL DEFAULT GETDATE(),
        is_winning BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_bids_auctions FOREIGN KEY (auction_id) REFERENCES auctions(id) ON DELETE CASCADE,
        CONSTRAINT FK_bids_bidder FOREIGN KEY (bidder_id) REFERENCES users(id)
    );
    
    CREATE INDEX IX_bids_auction ON bids(auction_id);
    CREATE INDEX IX_bids_bidder ON bids(bidder_id);
    
    PRINT 'Table bids created successfully';
END
ELSE
BEGIN
    PRINT 'Table bids already exists';
END
GO

-- Create trigger to update auction current_price when new bid is placed
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_update_auction_price')
BEGIN
    DROP TRIGGER trg_update_auction_price;
END
GO

CREATE TRIGGER trg_update_auction_price
ON bids
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update current_price in auctions table
    UPDATE a
    SET current_price = i.bid_amount,
        updated_at = GETDATE()
    FROM auctions a
    INNER JOIN inserted i ON a.id = i.auction_id;
    
    -- Mark all previous bids as not winning
    UPDATE b
    SET is_winning = 0
    FROM bids b
    INNER JOIN inserted i ON b.auction_id = i.auction_id
    WHERE b.id != i.id;
    
    -- Mark the new bid as winning
    UPDATE b
    SET is_winning = 1
    FROM bids b
    INNER JOIN inserted i ON b.id = i.id;
END
GO

PRINT 'Auction tables and triggers created successfully!';
PRINT '';
PRINT 'Tables created:';
PRINT '- auctions';
PRINT '- bids';
PRINT '';
PRINT 'Triggers created:';
PRINT '- trg_update_auction_price';
GO
