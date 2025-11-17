using BLL.DTOs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Concurrent; // 1. Dùng ConcurrentDictionary
using BLL.DTOs; // 2. Thêm DTO
using BLL.Helpers; // 3. Thêm Session Helper
namespace PRN222_FinalProject.Hubs;

public class NotificationHub : Hub
{
    // 4. Phải dùng ConcurrentDictionary để đảm bảo an toàn đa luồng
    private static readonly ConcurrentDictionary<int, string> _userConnections =
        new ConcurrentDictionary<int, string>();

    public override async Task OnConnectedAsync()
    {
        // 5. Lấy HttpContext
        var httpContext = Context.GetHttpContext();
        if (httpContext == null)
        {
            await base.OnConnectedAsync();
            return;
        }

        // 6. Đọc đúng key "CurrentUser"
        var currentUser = httpContext.Session.GetObjectFromJson<UserDto>("CurrentUser");

        if (currentUser != null)
        {
            // 7. Lưu kết nối
            _userConnections[currentUser.Id] = Context.ConnectionId;

            // 8. (Tùy chọn) Join group Admin ngay tại đây
            if (currentUser.Role == "admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
            // Add staff to Admins group for notifications
            else if (currentUser.Role == "staff")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var item = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (item.Key != 0)
        {
            // 9. Xóa khỏi ConcurrentDictionary
            _userConnections.TryRemove(item.Key, out _);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public static string? GetConnectionId(int userId)
    {
        return _userConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
    }

    // Send notification to specific user
    public async Task SendNotificationToUser(int userId, string message, string type)
    {
        var connectionId = GetConnectionId(userId);
        if (connectionId != null)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveNotification", message, type);
        }
    }

    // Send auction update to all users viewing the auction
    public async Task SendAuctionUpdate(int auctionId, object data)
    {
        await Clients.All.SendAsync("ReceiveAuctionUpdate", auctionId, data);
    }

    // Join auction-specific group
    public async Task JoinAuctionGroup(int auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Auction_{auctionId}");
        Console.WriteLine($"Connection {Context.ConnectionId} joined auction group {auctionId}");
    }

    // Leave auction-specific group
    public async Task LeaveAuctionGroup(int auctionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Auction_{auctionId}");
        Console.WriteLine($"Connection {Context.ConnectionId} left auction group {auctionId}");
    }

    // Test method
    public async Task TestConnection()
    {
        await Clients.Caller.SendAsync("TestResponse", "Hub connection successful");
    }

    // Simple test method
    public async Task SendTestMessage()
    {
        await Clients.All.SendAsync("AnyMessage", "Test message from hub at " + DateTime.Now.ToString("HH:mm:ss"));
    }

    // Send notification to all users
    public async Task SendNotificationToAll(string message, string type)
    {
        await Clients.All.SendAsync("ReceiveNotification", message, type);
    }

    // Send notification to admin users
    public async Task SendNotificationToAdmins(string message, string type)
    {
        await Clients.Group("Admins").SendAsync("ReceiveNotification", message, type);
    }

    // Join admin group
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
    }

    // Leave admin group
    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
    }
}
