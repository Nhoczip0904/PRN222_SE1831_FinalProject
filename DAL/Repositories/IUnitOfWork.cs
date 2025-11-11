namespace DAL.Repositories;

public interface IUnitOfWork : IDisposable
{
    // Repositories
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    ICategoryRepository Categories { get; }
    IAuctionRepository Auctions { get; }
    IBidRepository Bids { get; }
    IWalletRepository Wallets { get; }
    IWalletTransactionRepository WalletTransactions { get; }
    IContractRepository Contracts { get; }
    IFavoriteRepository Favorites { get; }
    IReviewRepository Reviews { get; }
    IMessageRepository Messages { get; }
    ISupportTicketRepository SupportTickets { get; }
    
    // Methods
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
