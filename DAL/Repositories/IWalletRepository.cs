using DAL.Entities;

namespace DAL.Repositories;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(int id);
    Task<Wallet?> GetByUserIdAsync(int userId);
    Task<Wallet> CreateAsync(Wallet wallet);
    Task<Wallet> UpdateAsync(Wallet wallet);
    Task<bool> UpdateBalanceAsync(int walletId, decimal newBalance);
}
