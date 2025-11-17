-- Add BidIncrement column to Auctions table
ALTER TABLE Auctions 
ADD BidIncrement decimal(18,2) NOT NULL DEFAULT 10000.00;
Go
-- Add constraint to ensure bid increment is positive
ALTER TABLE Auctions 
ADD CONSTRAINT CK_Auctions_BidIncrement_Positive 
CHECK (BidIncrement >= 1000.00);
