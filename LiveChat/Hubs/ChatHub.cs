using Microsoft.AspNetCore.SignalR;

namespace LiveChat.Hubs;

public class ChatHub : Hub
{
    // Dictionary to store connected users
    private static List<string> _activeUsers = new List<string>();

    // When a user connects, add them to the active users list
    public override async Task OnConnectedAsync()
    {
        var username = Context.GetHttpContext().Request.Query["username"].ToString();
        if (!string.IsNullOrEmpty(username) && !_activeUsers.Contains(username))
        {
            _activeUsers.Add(username);
        }

        // Notify all clients about the updated user list
        await Clients.All.SendAsync("UpdateActiveUsers", _activeUsers);

        await base.OnConnectedAsync();
    }

    // When a user disconnects, remove them from the active users list
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = Context.GetHttpContext().Request.Query["username"].ToString();
        if (!string.IsNullOrEmpty(username) && _activeUsers.Contains(username))
        {
            _activeUsers.Remove(username);
        }

        // Notify all clients about the updated user list
        await Clients.All.SendAsync("UpdateActiveUsers", _activeUsers);

        await base.OnDisconnectedAsync(exception);
    }

    // Send a message to a specific user
    public async Task SendMessage(string user, string message)
    {
        await Clients.User(user).SendAsync("ReceiveMessage", message);
    }
}
