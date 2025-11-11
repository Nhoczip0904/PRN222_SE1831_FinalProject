using DAL.Entities;

namespace DAL.Repositories;

public interface IWalletTransactionRepository
{
    Task<WalletTransaction?> GetByIdAsync(int id);
    Task<IEnumerable<WalletTransaction>> GetByWalletIdAsync(int walletId);
    Task<IEnumerable<WalletTransaction>> GetByUserIdAsync(int userId);
    Task<(IEnumerable<WalletTransaction> Transactions, int TotalCount)> GetByWalletIdWithPaginationAsync(int walletId, int pageNumber, int pageSize);
    Task<WalletTransaction> CreateAsync(WalletTransaction transaction);
}
