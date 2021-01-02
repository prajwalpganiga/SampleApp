using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using SampleApp.API.Data.Interfaces;
using SampleApp.API.Dtos;
using SampleApp.API.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleApp.API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IMapper _mapper;
        private readonly ISampleAppService _userService;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _presenceTracker;
        public MessageHub(IMessageService messageService, IMapper mapper, ISampleAppService userService,
            IHubContext<PresenceHub> presenceHub, PresenceTracker presenceTracker)
        {
            _messageService = messageService;
            _mapper = mapper;
            _userService = userService;
            _presenceHub = presenceHub;
            _presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.FindFirst(ClaimTypes.Name)?.Value, otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _messageService.GetMessageThread(Context.User.FindFirst(ClaimTypes.Name)?.Value, otherUser);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(ex);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.FindFirst(ClaimTypes.Name)?.Value;
            if (username == createMessageDto.RecipientUserName.ToLower())
                throw new HubException("You can not send messages to yourself");

            var sender = await _userService.GetUserByUserName(username);
            var recipient = await _userService.GetUserByUserName(createMessageDto.RecipientUserName);

            if (recipient == null) throw new HubException("User Not Found");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await _messageService.GetMessageGroup(groupName);
            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await _presenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new
                        {
                            username = sender.UserName,
                            knownAs = sender.KnownAs
                        });
                }
            }

            _messageService.AddMessage(message);

            if (await _messageService.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _messageService.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.FindFirst(ClaimTypes.Name)?.Value);

            if (group == null)
            {
                group = new Group(groupName);
                _messageService.AddGroup(group);
            }

            group.Connections.Add(connection);

            if( await _messageService.SaveAllAsync()) return group;

            throw new HubException("Failed to join Group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _messageService.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _messageService.RemoveConnection(connection);

            if(await _messageService.SaveAllAsync()) return group;

            throw new HubException("Failed to remove from group");
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}
