-- Tạo bảng contracts (Hợp đồng)
CREATE TABLE contracts (
    id INT PRIMARY KEY IDENTITY(1,1),
    order_id INT NOT NULL,
    buyer_id INT NOT NULL,
    seller_id INT NOT NULL,
    contract_number VARCHAR(50) NOT NULL UNIQUE,
    total_amount DECIMAL(18,2) NOT NULL,
    
    -- Trạng thái xác nhận
    buyer_confirmed BIT DEFAULT 0,
    buyer_confirmed_at DATETIME NULL,
    seller_confirmed BIT DEFAULT 0,
    seller_confirmed_at DATETIME NULL,
    admin_approved BIT DEFAULT 0,
    admin_approved_at DATETIME NULL,
    admin_approved_by INT NULL,
    
    -- Trạng thái hợp đồng
    status VARCHAR(20) DEFAULT 'pending', -- pending, confirmed, approved, rejected, cancelled
    rejection_reason NVARCHAR(500) NULL,
    
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_contracts_order FOREIGN KEY (order_id) REFERENCES orders(id),
    CONSTRAINT FK_contracts_buyer FOREIGN KEY (buyer_id) REFERENCES users(id),
    CONSTRAINT FK_contracts_seller FOREIGN KEY (seller_id) REFERENCES users(id),
    CONSTRAINT FK_contracts_admin FOREIGN KEY (admin_approved_by) REFERENCES users(id)
);

-- Tạo bảng contract_confirmations (Lịch sử xác nhận)
CREATE TABLE contract_confirmations (
    id INT PRIMARY KEY IDENTITY(1,1),
    contract_id INT NOT NULL,
    user_id INT NOT NULL,
    user_role VARCHAR(20) NOT NULL, -- buyer, seller, admin
    action VARCHAR(20) NOT NULL, -- confirmed, rejected, approved
    note NVARCHAR(500) NULL,
    ip_address VARCHAR(50) NULL,
    created_at DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_confirmations_contract FOREIGN KEY (contract_id) REFERENCES contracts(id),
    CONSTRAINT FK_confirmations_user FOREIGN KEY (user_id) REFERENCES users(id)
);

-- Tạo index
CREATE INDEX IX_contracts_order ON contracts(order_id);
CREATE INDEX IX_contracts_status ON contracts(status);
CREATE INDEX IX_confirmations_contract ON contract_confirmations(contract_id);

GO

PRINT 'Tạo bảng contracts và contract_confirmations thành công!';
