using BLL.DTOs;
using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;

    public WalletService(IWalletRepository walletRepository, IWalletTransactionRepository transactionRepository)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<WalletDto?> GetWalletByUserIdAsync(int userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null) return null;

        return new WalletDto
        {
            Id = wallet.Id,
            UserId = wallet.UserId,
            Balance = wallet.Balance,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt
        };
    }

    public async Task<WalletDto> GetOrCreateWalletAsync(int userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        
        if (wallet == null)
        {
            wallet = await _walletRepository.CreateAsync(new Wallet
            {
                UserId = userId,
                Balance = 0
            });
        }

        return new WalletDto
        {
            Id = wallet.Id,
            UserId = wallet.UserId,
            Balance = wallet.Balance,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt
        };
    }

    public async Task<IEnumerable<WalletTransactionDto>> GetTransactionHistoryAsync(int userId, int pageNumber = 1, int pageSize = 20)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null) return new List<WalletTransactionDto>();

        var result = await _transactionRepository.GetByWalletIdWithPaginationAsync(wallet.Id, pageNumber, pageSize);
        
        return result.Transactions.Select(t => new WalletTransactionDto
        {
            Id = t.Id,
            WalletId = t.WalletId,
            TransactionType = t.TransactionType,
            Amount = t.Amount,
            BalanceAfter = t.BalanceAfter,
            Description = t.Description,
            ReferenceId = t.ReferenceId,
            ReferenceType = t.ReferenceType,
            CreatedAt = t.CreatedAt
        });
    }

    public async Task<(bool Success, string Message, decimal NewBalance)> DepositAsync(int userId, DepositDto depositDto)
    {
        var wallet = await GetOrCreateWalletAsync(userId);
        var walletEntity = await _walletRepository.GetByIdAsync(wallet.Id);

        if (walletEntity == null)
        {
            return (false, "Không tìm thấy ví", 0);
        }

        var newBalance = walletEntity.Balance + depositDto.Amount;

        // Update balance
        await _walletRepository.UpdateBalanceAsync(wallet.Id, newBalance);

        // Record transaction
        await _transactionRepository.CreateAsync(new WalletTransaction
        {
            WalletId = wallet.Id,
            TransactionType = "deposit",
            Amount = depositDto.Amount,
            BalanceAfter = newBalance,
            Description = depositDto.Description ?? "Nạp tiền vào ví"
        });

        return (true, "Nạp tiền thành công", newBalance);
    }

    public async Task<(bool Success, string Message)> CheckBalanceAsync(int userId, decimal requiredAmount)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        
        if (wallet == null)
        {
            return (false, "Chưa có ví. Vui lòng tạo ví trước");
        }

        if (wallet.Balance < requiredAmount)
        {
            return (false, $"Số dư không đủ. Cần: {requiredAmount:N0} VND, Có: {wallet.Balance:N0} VND");
        }

        return (true, "Số dư đủ");
    }

    public async Task<(bool Success, string Message)> DeductBalanceAsync(int userId, decimal amount, string description, int? referenceId = null, string? referenceType = null)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        
        if (wallet == null)
        {
            return (false, "Không tìm thấy ví");
        }

        if (wallet.Balance < amount)
        {
            return (false, "Số dư không đủ");
        }

        var newBalance = wallet.Balance - amount;

        // Update balance
        await _walletRepository.UpdateBalanceAsync(wallet.Id, newBalance);

        // Record transaction
        await _transactionRepository.CreateAsync(new WalletTransaction
        {
            WalletId = wallet.Id,
            TransactionType = "payment",
            Amount = amount,
            BalanceAfter = newBalance,
            Description = description,
            ReferenceId = referenceId,
            ReferenceType = referenceType
        });

        return (true, "Trừ tiền thành công");
    }

    public async Task<(bool Success, string Message)> AddBalanceAsync(int userId, decimal amount, string description, int? referenceId = null, string? referenceType = null)
    {
        var wallet = await GetOrCreateWalletAsync(userId);
        var walletEntity = await _walletRepository.GetByIdAsync(wallet.Id);

        if (walletEntity == null)
        {
            return (false, "Không tìm thấy ví");
        }

        var newBalance = walletEntity.Balance + amount;

        // Update balance
        await _walletRepository.UpdateBalanceAsync(wallet.Id, newBalance);

        // Record transaction
        await _transactionRepository.CreateAsync(new WalletTransaction
        {
            WalletId = wallet.Id,
            TransactionType = "refund",
            Amount = amount,
            BalanceAfter = newBalance,
            Description = description,
            ReferenceId = referenceId,
            ReferenceType = referenceType
        });

        return (true, "Cộng tiền thành công");
    }
}
