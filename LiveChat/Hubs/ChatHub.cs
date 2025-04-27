using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace LiveChat.Hubs;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnectionMap = new();

    public async Task Join(string username)
    {
        var connectionId = Context.ConnectionId;
        _userConnectionMap.TryGetValue(username, out var list);
        _userConnectionMap.AddOrUpdate(
        username,
        new HashSet<string> { connectionId },
        (key, existingList) =>
        {
            existingList.Add(connectionId);
            return existingList;
        });

        //await Clients.All.SendAsync("UserJoined", username);
        await Clients.All.SendAsync("UpdateActiveUsers", _userConnectionMap.Keys);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = _userConnectionMap
            .FirstOrDefault(x => x.Value.Contains(Context.ConnectionId)).Key;

        if (!string.IsNullOrEmpty(username))
        {
            var values = _userConnectionMap[username];
            values.Remove(Context.ConnectionId);
            if (values.Count > 0)
            {
                _userConnectionMap[username] = values;
            }
            else
            {
                _userConnectionMap.TryRemove(username, out values);
                
                // Notify everyone that user left
                await Clients.All.SendAsync("UserLeft", username);
            }

        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string fromUser, string toUser, string message)
    {
        if (string.IsNullOrWhiteSpace(toUser))
        {
            // Send to everyone
            await Clients.All.SendAsync("ReceiveMessage", fromUser, message);
        }
        else
        {
            if (_userConnectionMap.TryGetValue(toUser, out var receiverConnectionIds) && _userConnectionMap.TryGetValue(fromUser, out HashSet<string> senderIds))
            {
                // Combine senderIds and receiverConnectionIds into one list
                var allConnectionIds = new HashSet<string>();
                allConnectionIds.UnionWith(senderIds);
                allConnectionIds.UnionWith(receiverConnectionIds);

                // Send to both sender and receiver
                await Clients.Clients(allConnectionIds)
                    .SendAsync("ReceiveMessage", fromUser, message);
            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveSystemMessage", $"{toUser} is no longer online.");
            }
        }
    }

    public async Task ShowUserTyping(string fromUser, string toUser, bool show)
    {
        if (string.IsNullOrWhiteSpace(toUser))
        {
            // Send to everyone
            await Clients.Others.SendAsync("ShowUserTyping", fromUser, toUser, show);
            return;
        }

        if (_userConnectionMap.TryGetValue(toUser, out var receiverConnectionId) &&
            _userConnectionMap.TryGetValue(fromUser, out var senderConnectionId))
        {
            // Send to both sender and receiver
            await Clients.Clients(receiverConnectionId)
                .SendAsync("ShowUserTyping", fromUser, toUser, show);
        }
        else
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", $"{toUser} is no longer online.");
        }
    }
}