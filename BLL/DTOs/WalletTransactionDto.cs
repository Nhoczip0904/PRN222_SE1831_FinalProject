namespace BLL.DTOs;

public class WalletTransactionDto
{
    public int Id { get; set; }
    public int WalletId { get; set; }
    public string TransactionType { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Description { get; set; }
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public DateTime CreatedAt { get; set; }
}
