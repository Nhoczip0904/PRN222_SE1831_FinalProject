using BLL.DTOs;

namespace BLL.Services;

public interface IWalletService
{
    Task<WalletDto?> GetWalletByUserIdAsync(int userId);
    Task<WalletDto> GetOrCreateWalletAsync(int userId);
    Task<IEnumerable<WalletTransactionDto>> GetTransactionHistoryAsync(int userId, int pageNumber = 1, int pageSize = 20);
    Task<(bool Success, string Message, decimal NewBalance)> DepositAsync(int userId, DepositDto depositDto);
    Task<(bool Success, string Message)> CheckBalanceAsync(int userId, decimal requiredAmount);
    Task<(bool Success, string Message)> DeductBalanceAsync(int userId, decimal amount, string description, int? referenceId = null, string? referenceType = null);
    Task<(bool Success, string Message)> AddBalanceAsync(int userId, decimal amount, string description, int? referenceId = null, string? referenceType = null);
}
