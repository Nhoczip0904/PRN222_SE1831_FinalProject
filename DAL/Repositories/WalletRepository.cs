using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly EvBatteryTrading2Context _context;

    public WalletRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetByIdAsync(int id)
    {
        return await _context.Wallets
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Wallet?> GetByUserIdAsync(int userId)
    {
        return await _context.Wallets
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task<Wallet> CreateAsync(Wallet wallet)
    {
        wallet.CreatedAt = DateTime.Now;
        wallet.UpdatedAt = DateTime.Now;
        wallet.Balance = 0;

        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<Wallet> UpdateAsync(Wallet wallet)
    {
        wallet.UpdatedAt = DateTime.Now;

        _context.Wallets.Update(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<bool> UpdateBalanceAsync(int walletId, decimal newBalance)
    {
        var wallet = await GetByIdAsync(walletId);
        if (wallet == null)
            return false;

        wallet.Balance = newBalance;
        wallet.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }
}
