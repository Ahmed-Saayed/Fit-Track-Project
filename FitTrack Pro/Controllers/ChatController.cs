using FitTrack_Pro.Common;
using FitTrack_Pro.Hubs;
using FitTrack_Pro.Models;
using FitTrack_Pro.Services;
using FitTrack_Pro.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FitTrack_Pro.Controllers
{
    public class ChatController(IChatService chatService, IHubContext<ChatHub> hubContext) : Controller
    {
        private readonly IHubContext<ChatHub> _hubContext = hubContext;
        private readonly IChatService _chatService = chatService;
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> getChatsMetaDataForAdmin()
        {
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ResponseModel<IEnumerable<ChatsViewModel>> result = await _chatService.getChatsAsync(currentUserId);

            if (result.IsSuccess)
            {
                return View(result.Data);
            }
            else
            {
                return View(null);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Member,Trainer")]

        public async Task<IActionResult> getChatsMetaDataForUser()
        {
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ResponseModel<IEnumerable<ChatsViewModel>> result = await _chatService.getChatsV2Async(currentUserId);

            if (result.IsSuccess)
            {
                return View("getChatsMetaDataForAdmin", result.Data);
            }
            else
            {
                return View("getChatsMetaDataForAdmin", null);
            }
        }

        [HttpPost]
        [Authorize]

        public async Task<IActionResult> sendNewMessage([FromBody] MessageRequestViewModel messageRequest)
        {
            if (ModelState.IsValid)
            {
                ResponseModel<Message> result = await _chatService.AddMessage(messageRequest);
                if (result.IsSuccess)
                {
                    MessageViewModel msg = new MessageViewModel
                    {
                        Id = result.Data.Id,
                        SenderId = messageRequest.SenderId,
                        Message = messageRequest.Message,
                        SenderName = User.Identity.Name,
                        SentAt = result.Data.CreatedAt
                    };
                    await _hubContext.Clients.All.SendAsync("RecieveMessage", msg, messageRequest.ReciverId);
                    return Ok();
                }

                else return BadRequest();
            }
            return BadRequest();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetChatMessages(string UserId, string FullName)
        {
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ResponseModel<IEnumerable<MessageViewModel>> messages = await _chatService.getChatMessages(currentUserId, UserId);
            TempData["FullName"] = FullName;
            return View(messages.Data);
        }

        [HttpPut]
        [Authorize]

        public async Task<IActionResult> UpdateMessage(int MessageId, [FromBody] UpdateMessageViewModel request)
        {
            if (ModelState.IsValid)
            {
                await _hubContext.Clients.All.SendAsync("UpdateMessage", MessageId, request.MesssageContent);
                ResponseModel<bool> result = await _chatService.UpdateMessageMessage(MessageId, request);
                if (result.IsSuccess)
                {
                    return Ok();
                }
                else return BadRequest();
            }
            return BadRequest();
        }

        [HttpDelete]
        [Authorize]

        public async Task<IActionResult> DeleteMessage(int MessageId)
        {
            ResponseModel<bool> result = await _chatService.DeleteMessage(MessageId);
            if (result.IsSuccess)
            {
                await _hubContext.Clients.All.SendAsync("DeleteMessage", MessageId);
                return Ok();
            }
            else return BadRequest();
        }
    }
}
