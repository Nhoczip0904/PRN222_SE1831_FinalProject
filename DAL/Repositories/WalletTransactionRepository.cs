using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class WalletTransactionRepository : IWalletTransactionRepository
{
    private readonly EvBatteryTrading2Context _context;

    public WalletTransactionRepository(EvBatteryTrading2Context context)
    {
        _context = context;
    }

    public async Task<WalletTransaction?> GetByIdAsync(int id)
    {
        return await _context.WalletTransactions
            .Include(wt => wt.Wallet)
            .FirstOrDefaultAsync(wt => wt.Id == id);
    }

    public async Task<IEnumerable<WalletTransaction>> GetByWalletIdAsync(int walletId)
    {
        return await _context.WalletTransactions
            .Where(wt => wt.WalletId == walletId)
            .OrderByDescending(wt => wt.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<WalletTransaction>> GetByUserIdAsync(int userId)
    {
        return await _context.WalletTransactions
            .Include(wt => wt.Wallet)
            .Where(wt => wt.Wallet.UserId == userId)
            .OrderByDescending(wt => wt.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<WalletTransaction> Transactions, int TotalCount)> GetByWalletIdWithPaginationAsync(int walletId, int pageNumber, int pageSize)
    {
        var query = _context.WalletTransactions
            .Where(wt => wt.WalletId == walletId);

        var totalCount = await query.CountAsync();
        var transactions = await query
            .OrderByDescending(wt => wt.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (transactions, totalCount);
    }

    public async Task<WalletTransaction> CreateAsync(WalletTransaction transaction)
    {
        transaction.CreatedAt = DateTime.Now;

        _context.WalletTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }
}
