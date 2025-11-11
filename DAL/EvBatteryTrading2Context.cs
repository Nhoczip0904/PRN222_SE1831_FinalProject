using System;
using System.Collections.Generic;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DAL;

public partial class EvBatteryTrading2Context : DbContext
{
    public EvBatteryTrading2Context()
    {
    }

    public EvBatteryTrading2Context(DbContextOptions<EvBatteryTrading2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Auction> Auctions { get; set; }

    public virtual DbSet<Bid> Bids { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<ContractConfirmation> ContractConfirmations { get; set; }

    // 7 Steps Business Logic Features
    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<SellerRating> SellerRatings { get; set; }

    public virtual DbSet<SupportTicket> SupportTickets { get; set; }

    public virtual DbSet<TicketMessage> TicketMessages { get; set; }

    public virtual DbSet<SystemRevenue> SystemRevenues { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string will be configured in Program.cs via Dependency Injection
        // This method is only used when DbContext is created without DI (e.g., migrations)
        if (!optionsBuilder.IsConfigured)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__categori__3213E83F91DBC08F");

            entity.ToTable("categories", tb => tb.HasTrigger("trg_update_categories"));

            entity.HasIndex(e => e.Name, "UQ__categori__72E12F1BB14B8604").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orders__3213E83FEEEBDF5A");

            entity.ToTable("orders", tb => tb.HasTrigger("trg_update_orders"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.ShippingAddress).HasColumnName("shipping_address");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Buyer).WithMany(p => p.OrderBuyers)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_orders_buyer");

            entity.HasOne(d => d.Seller).WithMany(p => p.OrderSellers)
                .HasForeignKey(d => d.SellerId)
                .HasConstraintName("FK_orders_seller");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__order_it__3213E83FD76380BD");

            entity.ToTable("order_items");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_order_items_order");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_order_items_product");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__products__3213E83F52D24F81");

            entity.ToTable("products", tb => tb.HasTrigger("trg_update_products"));

            entity.HasIndex(e => e.CategoryId, "idx_products_category");

            entity.HasIndex(e => e.SellerId, "idx_products_seller");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BatteryHealthPercent).HasColumnName("battery_health_percent");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Condition)
                .HasMaxLength(20)
                .HasColumnName("condition");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Images).HasColumnName("images");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsSold)
                .HasDefaultValue(false)
                .HasColumnName("is_sold");
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(20)
                .HasColumnName("approval_status");
            entity.Property(e => e.ApprovedBy)
                .HasColumnName("approved_by");
            entity.Property(e => e.ApprovedAt)
                .HasColumnType("datetime")
                .HasColumnName("approved_at");
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500)
                .HasColumnName("rejection_reason");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_products_category");

            entity.HasOne(d => d.Seller).WithMany(p => p.Products)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_products_seller");

            entity.HasOne(d => d.Approver).WithMany()
                .HasForeignKey(d => d.ApprovedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_products_approved_by");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transact__3213E83F890B2CB1");

            entity.ToTable("transactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
                .HasColumnName("payment_method");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(255)
                .HasColumnName("transaction_id");

            entity.HasOne(d => d.Order).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_transactions_order");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83FCE5AED7F");

            entity.ToTable("users", tb => tb.HasTrigger("trg_update_users"));

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164B3AE3AB4").IsUnique();

            entity.HasIndex(e => e.Phone, "UQ__users__B43B145F715F8837").IsUnique();

            entity.HasIndex(e => e.Email, "idx_users_email");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.IsVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_verified");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("member")
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Auction>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("auctions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.StartingPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("starting_price");
            entity.Property(e => e.CurrentPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("current_price");
            entity.Property(e => e.ReservePrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("reserve_price");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.WinnerId).HasColumnName("winner_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Seller)
                .WithMany()
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Winner)
                .WithMany()
                .HasForeignKey(d => d.WinnerId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("bids", tb => tb.HasTrigger("trg_update_auction_price"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuctionId).HasColumnName("auction_id");
            entity.Property(e => e.BidderId).HasColumnName("bidder_id");
            entity.Property(e => e.BidAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("bid_amount");
            entity.Property(e => e.BidTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("bid_time");
            entity.Property(e => e.IsWinning)
                .HasDefaultValue(false)
                .HasColumnName("is_winning");

            entity.HasOne(d => d.Auction)
                .WithMany(p => p.Bids)
                .HasForeignKey(d => d.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Bidder)
                .WithMany()
                .HasForeignKey(d => d.BidderId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("wallets", tb => tb.HasTrigger("trg_update_wallets"));

            entity.HasIndex(e => e.UserId).IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(18, 2)")
                .HasDefaultValue(0)
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.User)
                .WithOne()
                .HasForeignKey<Wallet>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("wallet_transactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .HasColumnName("transaction_type");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BalanceAfter)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("balance_after");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(20)
                .HasColumnName("reference_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Wallet)
                .WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("contracts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.ContractNumber).HasMaxLength(50).HasColumnName("contract_number");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").HasColumnName("total_amount");

            entity.Property(e => e.BuyerConfirmed).HasColumnName("buyer_confirmed");
            entity.Property(e => e.BuyerConfirmedAt).HasColumnType("datetime").HasColumnName("buyer_confirmed_at");
            entity.Property(e => e.SellerConfirmed).HasColumnName("seller_confirmed");
            entity.Property(e => e.SellerConfirmedAt).HasColumnType("datetime").HasColumnName("seller_confirmed_at");
            entity.Property(e => e.AdminApproved).HasColumnName("admin_approved");
            entity.Property(e => e.AdminApprovedAt).HasColumnType("datetime").HasColumnName("admin_approved_at");
            entity.Property(e => e.AdminApprovedBy).HasColumnName("admin_approved_by");

            entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status");
            entity.Property(e => e.RejectionReason).HasMaxLength(500).HasColumnName("rejection_reason");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasColumnName("updated_at");

            entity.HasOne(d => d.Order).WithMany().HasForeignKey(d => d.OrderId);
            entity.HasOne(d => d.Buyer).WithMany().HasForeignKey(d => d.BuyerId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Seller).WithMany().HasForeignKey(d => d.SellerId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.AdminApprover).WithMany().HasForeignKey(d => d.AdminApprovedBy).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ContractConfirmation>(entity =>
        {
            entity.ToTable("contract_confirmations");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContractId).HasColumnName("contract_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserRole).HasMaxLength(20).HasColumnName("user_role");
            entity.Property(e => e.Action).HasMaxLength(20).HasColumnName("action");
            entity.Property(e => e.Note).HasMaxLength(500).HasColumnName("note");
            entity.Property(e => e.IpAddress).HasMaxLength(50).HasColumnName("ip_address");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");

            entity.HasOne(d => d.Contract).WithMany(p => p.Confirmations).HasForeignKey(d => d.ContractId);
            entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.ToTable("support_tickets");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TicketNumber).HasMaxLength(50).HasColumnName("ticket_number");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Category).HasMaxLength(50).HasColumnName("category");
            entity.Property(e => e.Subject).HasMaxLength(255).HasColumnName("subject");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Images).HasColumnName("images");
            entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status");
            entity.Property(e => e.Priority).HasMaxLength(20).HasColumnName("priority");
            entity.Property(e => e.AssignedTo).HasColumnName("assigned_to");
            entity.Property(e => e.AdminNotes).HasColumnName("admin_notes");
            entity.Property(e => e.Resolution).HasColumnName("resolution");
            entity.Property(e => e.ResolvedAt).HasColumnType("datetime").HasColumnName("resolved_at");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasColumnName("updated_at");

            // Configure relationships with explicit foreign keys
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.AssignedAdmin)
                .WithMany()
                .HasForeignKey(d => d.AssignedTo)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Order)
                .WithMany()
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TicketMessage>(entity =>
        {
            entity.ToTable("ticket_messages");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Attachments).HasColumnName("attachments");
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");

            entity.HasOne(d => d.Ticket)
                .WithMany(p => p.Messages)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SystemRevenue>(entity =>
        {
            entity.ToTable("system_revenues");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OrderAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("order_amount");
            entity.Property(e => e.CommissionRate)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("commission_rate");
            entity.Property(e => e.CommissionAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("commission_amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Order)
                .WithMany()
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
