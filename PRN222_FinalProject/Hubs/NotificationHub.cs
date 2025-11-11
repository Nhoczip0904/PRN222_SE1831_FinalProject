using Microsoft.AspNetCore.SignalR;

namespace PRN222_FinalProject.Hubs;

public class NotificationHub : Hub
{
    private static readonly Dictionary<int, string> _userConnections = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Session.GetInt32("UserId");
        if (userId.HasValue)
        {
            _userConnections[userId.Value] = Context.ConnectionId;
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
        if (userId != 0)
        {
            _userConnections.Remove(userId);
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
