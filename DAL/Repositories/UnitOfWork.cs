using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly EvBatteryTrading2Context _context;
    private IDbContextTransaction? _transaction;
    
    // Repository instances
    private IUserRepository? _users;
    private IProductRepository? _products;
    private IOrderRepository? _orders;
    private ICategoryRepository? _categories;
    private IAuctionRepository? _auctions;
    private IBidRepository? _bids;
    private IWalletRepository? _wallets;
    private IWalletTransactionRepository? _walletTransactions;
    private IContractRepository? _contracts;
    private IFavoriteRepository? _favorites;
    private IReviewRepository? _reviews;
    private IMessageRepository? _messages;
    private ISupportTicketRepository? _supportTickets;

    public UnitOfWork(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    // Lazy initialization of repositories
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IAuctionRepository Auctions => _auctions ??= new AuctionRepository(_context);
    public IBidRepository Bids => _bids ??= new BidRepository(_context);
    public IWalletRepository Wallets => _wallets ??= new WalletRepository(_context);
    public IWalletTransactionRepository WalletTransactions => _walletTransactions ??= new WalletTransactionRepository(_context);
    public IContractRepository Contracts => _contracts ??= new ContractRepository(_context);
    public IFavoriteRepository Favorites => _favorites ??= new FavoriteRepository(_context);
    public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);
    public IMessageRepository Messages => _messages ??= new MessageRepository(_context);
    public ISupportTicketRepository SupportTickets => _supportTickets ??= new SupportTicketRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
