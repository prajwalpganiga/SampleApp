using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleApp.API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            var isOnline = await _tracker.UserConnected(Context.User.FindFirst(ClaimTypes.Name)?.Value, Context.ConnectionId);
            if(isOnline)
            {
                await Clients.Others.SendAsync("UserIsOnline", Context.User.FindFirst(ClaimTypes.Name)?.Value);
            }

            var currentUsers = await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string username = Context.User.FindFirst(ClaimTypes.Name)?.Value;
            var isOffline = await _tracker.UserDisconnected(username, Context.ConnectionId);
            if(isOffline)
            {
                await Clients.Others.SendAsync("UserIsOffline", username);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
