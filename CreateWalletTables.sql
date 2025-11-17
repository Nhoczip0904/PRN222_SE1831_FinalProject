-- Create Wallet System for EV Battery Trading Platform
USE ev_battery_trading2;
GO

-- Create Wallets table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'wallets')
BEGIN
    CREATE TABLE wallets (
        id INT IDENTITY(1,1) PRIMARY KEY,
        user_id INT NOT NULL UNIQUE,
        balance DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (balance >= 0),
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_wallets_users FOREIGN KEY (user_id) REFERENCES users(id)
    );
    
    PRINT 'Table wallets created successfully';
END
ELSE
BEGIN
    PRINT 'Table wallets already exists';
END
GO

-- Create Wallet Transactions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'wallet_transactions')
BEGIN
    CREATE TABLE wallet_transactions (
        id INT IDENTITY(1,1) PRIMARY KEY,
        wallet_id INT NOT NULL,
        transaction_type VARCHAR(20) NOT NULL CHECK (transaction_type IN ('deposit', 'withdraw', 'bid_hold', 'bid_release', 'payment', 'refund')),
        amount DECIMAL(18,2) NOT NULL CHECK (amount > 0),
        balance_after DECIMAL(18,2) NOT NULL,
        description NVARCHAR(500),
        reference_id INT NULL, -- auction_id or order_id
        reference_type VARCHAR(20) NULL, -- 'auction', 'order'
        created_at DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_wallet_transactions_wallets FOREIGN KEY (wallet_id) REFERENCES wallets(id)
    );
    
    CREATE INDEX IX_wallet_transactions_wallet ON wallet_transactions(wallet_id);
    CREATE INDEX IX_wallet_transactions_type ON wallet_transactions(transaction_type);
    
    PRINT 'Table wallet_transactions created successfully';
END
ELSE
BEGIN
    PRINT 'Table wallet_transactions already exists';
END
GO

-- Create trigger to update wallet updated_at
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_update_wallets')
BEGIN
    DROP TRIGGER trg_update_wallets;
END
GO

CREATE TRIGGER trg_update_wallets
ON wallets
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE wallets
    SET updated_at = GETDATE()
    FROM wallets w
    INNER JOIN inserted i ON w.id = i.id;
END
GO

-- Create stored procedure to deposit money
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_deposit_money')
BEGIN
    DROP PROCEDURE sp_deposit_money;
END
GO

CREATE PROCEDURE sp_deposit_money
    @user_id INT,
    @amount DECIMAL(18,2),
    @description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @wallet_id INT;
        DECLARE @new_balance DECIMAL(18,2);
        
        -- Get or create wallet
        SELECT @wallet_id = id FROM wallets WHERE user_id = @user_id;
        
        IF @wallet_id IS NULL
        BEGIN
            INSERT INTO wallets (user_id, balance) VALUES (@user_id, 0);
            SET @wallet_id = SCOPE_IDENTITY();
        END
        
        -- Update balance
        UPDATE wallets 
        SET balance = balance + @amount,
            @new_balance = balance + @amount
        WHERE id = @wallet_id;
        
        -- Record transaction
        INSERT INTO wallet_transactions (wallet_id, transaction_type, amount, balance_after, description)
        VALUES (@wallet_id, 'deposit', @amount, @new_balance, @description);
        
        COMMIT TRANSACTION;
        SELECT @new_balance AS new_balance;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Create stored procedure to hold money for bid
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_hold_bid_amount')
BEGIN
    DROP PROCEDURE sp_hold_bid_amount;
END
GO

CREATE PROCEDURE sp_hold_bid_amount
    @user_id INT,
    @auction_id INT,
    @amount DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @wallet_id INT;
        DECLARE @current_balance DECIMAL(18,2);
        DECLARE @new_balance DECIMAL(18,2);
        
        -- Get wallet
        SELECT @wallet_id = id, @current_balance = balance 
        FROM wallets 
        WHERE user_id = @user_id;
        
        IF @wallet_id IS NULL
        BEGIN
            RAISERROR('Wallet not found', 16, 1);
            RETURN;
        END
        
        IF @current_balance < @amount
        BEGIN
            RAISERROR('Insufficient balance', 16, 1);
            RETURN;
        END
        
        -- Deduct balance
        UPDATE wallets 
        SET balance = balance - @amount,
            @new_balance = balance - @amount
        WHERE id = @wallet_id;
        
        -- Record transaction
        INSERT INTO wallet_transactions (wallet_id, transaction_type, amount, balance_after, description, reference_id, reference_type)
        VALUES (@wallet_id, 'bid_hold', @amount, @new_balance, N'Giữ tiền cho đấu giá', @auction_id, 'auction');
        
        COMMIT TRANSACTION;
        SELECT @new_balance AS new_balance;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Wallet system created successfully!';
PRINT '';
PRINT 'Tables created:';
PRINT '- wallets';
PRINT '- wallet_transactions';
PRINT '';
PRINT 'Stored Procedures created:';
PRINT '- sp_deposit_money';
PRINT '- sp_hold_bid_amount';
GO
