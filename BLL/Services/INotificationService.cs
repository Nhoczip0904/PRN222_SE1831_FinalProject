namespace BLL.Services;

/// <summary>
/// Interface for notification service - follows 3-layer architecture
/// Implementation is in Presentation layer (PRN222_FinalProject)
/// </summary>
public interface INotificationService
{
    Task NotifyProductApprovalAsync(int sellerId, int productId, string productName, bool approved);
    Task NotifyOrderStatusChangeAsync(int buyerId, int sellerId, int orderId, string newStatus);
    Task NotifyContractApprovalAsync(int buyerId, int sellerId, int contractId, bool approved);
    Task NotifyNewOrderAsync(int sellerId, int orderId, decimal amount);
    Task NotifyNewBidAsync(int sellerId, int auctionId, decimal bidAmount);
    Task NotifyAuctionWinnerAsync(int winnerId, int auctionId, string productName, decimal winningBid);
    Task NotifyAdminNewProductAsync(int productId, string productName, string sellerName);
    Task NotifyAdminNewOrderAsync(int orderId, decimal amount);
    Task NotifyAdminNewContractAsync(int contractId, int orderId);
    Task NotifyPaymentReceivedAsync(int sellerId, decimal amount, int orderId);
    Task BroadcastNewProductAsync(int productId, string productName, decimal price, string imageUrl);
    Task BroadcastProductRemovedAsync(int productId, string reason);
}
