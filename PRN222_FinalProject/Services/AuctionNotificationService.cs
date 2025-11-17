using BLL.Services;
using Microsoft.AspNetCore.SignalR;
using PRN222_FinalProject.Hubs;

namespace PRN222_FinalProject.Services;

public class AuctionNotificationService : IAuctionNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public AuctionNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendAuctionUpdateAsync(int auctionId, object data)
    {
        Console.WriteLine($"=== SENDING AUCTION UPDATE ===");
        Console.WriteLine($"Auction ID: {auctionId}");
        Console.WriteLine($"Data: {System.Text.Json.JsonSerializer.Serialize(data)}");
        
        // Send to auction-specific group
        await _hubContext.Clients.Group($"Auction_{auctionId}").SendAsync("ReceiveAuctionUpdate", auctionId, data);
        
        // Also send to all for debugging
        await _hubContext.Clients.All.SendAsync("ReceiveAuctionUpdate", auctionId, data);
        
        Console.WriteLine("=== AUCTION UPDATE SENT ===");
    }
}
