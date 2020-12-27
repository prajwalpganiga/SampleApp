using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleApp.API.Data.Interfaces;
using SampleApp.API.Dtos;
using SampleApp.API.Extensions;
using SampleApp.API.Helpers;
using SampleApp.API.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ISampleAppService _userService;
        private readonly IMessageService _messageService;
        private readonly IMapper _mapper;

        public MessagesController(ISampleAppService userService, IMessageService messageService,
            IMapper mapper)
        {
            _userService = userService;
            _messageService = messageService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (username == createMessageDto.RecipientUserName.ToLower())
                return BadRequest("You can not send messages to yourself");

            var sender = await _userService.GetUserByUserName(username);
            var recipient = await _userService.GetUserByUserName(createMessageDto.RecipientUserName);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.Username,
                RecipientUsername = recipient.Username,
                Content = createMessageDto.Content
            };

            _messageService.AddMessage(message);

            if (await _messageService.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.FindFirst(ClaimTypes.Name)?.Value;

            var messages = await _messageService.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.FindFirst(ClaimTypes.Name)?.Value;

            return Ok(_messageService.GetMessageThread(currentUsername, username));
        }
    }
}
