using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.SignalR;
using PRN222_FinalProject.Hubs;

namespace PRN222_FinalProject.Services;

/// <summary>
/// Implementation of INotificationService using SignalR
/// This is in Presentation layer because it depends on SignalR (web infrastructure)
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyProductApprovalAsync(int sellerId, int productId, string productName, bool approved, string? reason = null)
    {
        var connectionId = NotificationHub.GetConnectionId(sellerId);
        if (connectionId != null)
        {
            var message = approved 
                ? $"Sản phẩm '{productName}' đã được duyệt!" 
                : $"Sản phẩm '{productName}' đã bị từ chối.{(!string.IsNullOrEmpty(reason) ? $" Lý do: {reason}" : "")}";
            var type = approved ? "success" : "error";
            
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", message, type, $"/Products/Details?id={productId}");
        }
    }

    public async Task NotifyOrderStatusChangeAsync(int buyerId, int sellerId, int orderId, string newStatus)
    {
        var statusText = newStatus switch
        {
            "confirmed" => "đã được xác nhận",
            "shipped" => "đang được giao",
            "delivered" => "đã giao thành công",
            "cancelled" => "đã bị hủy",
            _ => $"có trạng thái mới: {newStatus}"
        };

        // Notify buyer
        var buyerConnectionId = NotificationHub.GetConnectionId(buyerId);
        if (buyerConnectionId != null)
        {
            await _hubContext.Clients.Client(buyerConnectionId).SendAsync(
                "ReceiveNotification", 
                $"Đơn hàng #{orderId} {statusText}", 
                "info",
                $"/Orders/Index"
            );
        }

        // Notify seller
        var sellerConnectionId = NotificationHub.GetConnectionId(sellerId);
        if (sellerConnectionId != null)
        {
            await _hubContext.Clients.Client(sellerConnectionId).SendAsync(
                "ReceiveNotification", 
                $"Đơn hàng #{orderId} {statusText}", 
                "info",
                $"/Orders/Index?viewType=seller"
            );
        }
    }

    public async Task NotifyContractApprovalAsync(int buyerId, int sellerId, int contractId, bool approved)
    {
        var message = approved 
            ? $"Hợp đồng #{contractId} đã được admin duyệt!" 
            : $"Hợp đồng #{contractId} đã bị từ chối.";
        var type = approved ? "success" : "error";

        // Notify buyer
        var buyerConnectionId = NotificationHub.GetConnectionId(buyerId);
        if (buyerConnectionId != null)
        {
            await _hubContext.Clients.Client(buyerConnectionId).SendAsync(
                "ReceiveNotification", 
                message, 
                type,
                $"/Contracts/Details?id={contractId}"
            );
        }

        // Notify seller
        var sellerConnectionId = NotificationHub.GetConnectionId(sellerId);
        if (sellerConnectionId != null)
        {
            await _hubContext.Clients.Client(sellerConnectionId).SendAsync(
                "ReceiveNotification", 
                message, 
                type,
                $"/Contracts/Details?id={contractId}"
            );
        }
    }

    public async Task NotifyNewOrderAsync(int sellerId, int orderId, decimal amount)
    {
        var connectionId = NotificationHub.GetConnectionId(sellerId);
        if (connectionId != null)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync(
                "ReceiveNotification", 
                $"Bạn có đơn hàng mới #{orderId} - {amount:N0} đ", 
                "success",
                $"/Orders/Index?viewType=seller"
            );
        }
    }

    public async Task NotifyNewBidAsync(int sellerId, int auctionId, decimal bidAmount)
    {
        Console.WriteLine($"[DEBUG] Auction null với AuctionId: {NotificationHub.GetConnectionId(sellerId)}");
    
    var connectionId = NotificationHub.GetConnectionId(sellerId);
        if (connectionId != null)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync(
                "ReceiveNotification", 
                $"Có người đặt giá {bidAmount:N0} đ cho đấu giá #{auctionId}", 
                "info",
                $"/Auctions/Details?id={auctionId}"
            );
        }
    }

    public async Task NotifyAuctionWinnerAsync(int winnerId, int auctionId, string productName, decimal winningBid)
    {
        var connectionId = NotificationHub.GetConnectionId(winnerId);
        if (connectionId != null)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync(
                "ReceiveNotification", 
                $"Chúc mừng! Bạn đã thắng đấu giá '{productName}' với giá {winningBid:N0} đ", 
                "success",
                $"/Orders/Index"
            );
        }
    }

    public async Task NotifyAdminNewProductAsync(int productId, string productName, string sellerName)
    {
        await _hubContext.Clients.Group("Admins").SendAsync(
            "ReceiveNotification", 
            $"Sản phẩm mới '{productName}' từ {sellerName} cần duyệt", 
            "warning",
            $"/Admin/Products/Details?id={productId}"
        );
    }

    public async Task NotifyAdminNewOrderAsync(int orderId, decimal amount)
    {
        await _hubContext.Clients.Group("Admins").SendAsync(
            "ReceiveNotification", 
            $"Đơn hàng mới #{orderId} - {amount:N0} đ", 
            "info",
            $"/Admin/Orders/Details?id={orderId}"
        );
    }

    public async Task NotifyAdminNewContractAsync(int contractId, int orderId)
    {
        await _hubContext.Clients.Group("Admins").SendAsync(
            "ReceiveNotification", 
            $"Hợp đồng mới #{contractId} cho đơn hàng #{orderId} cần duyệt", 
            "warning",
            $"/Admin/Contracts/Details?id={contractId}"
        );
    }

    public async Task NotifyPaymentReceivedAsync(int sellerId, decimal amount, int orderId)
    {
        var connectionId = NotificationHub.GetConnectionId(sellerId);
        if (connectionId != null)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync(
                "ReceiveNotification", 
                $"Bạn đã nhận được {amount:N0} đ từ đơn hàng #{orderId}", 
                "success",
                $"/Wallet/Index"
            );
        }
    }

    public async Task BroadcastNewProductAsync(int productId, string productName, decimal price, string imageUrl)
    {
        // Broadcast to ALL connected clients (not just specific users)
        await _hubContext.Clients.All.SendAsync(
            "NewProductAvailable",
            new
            {
                productId,
                productName,
                price,
                imageUrl,
                message = $"Sản phẩm mới: {productName} - {price:N0} đ"
            }
        );
    }

    public async Task BroadcastProductRemovedAsync(int productId, string reason)
    {
        // Broadcast to ALL connected clients to remove product from UI
        await _hubContext.Clients.All.SendAsync(
            "ProductRemoved",
            new
            {
                productId,
                reason,
                message = $"Sản phẩm đã bị gỡ: {reason}"
            }
        );
    }
}
