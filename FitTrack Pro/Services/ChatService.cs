using Common;
using FitTrack_Pro.Common;
using FitTrack_Pro.Models;
using FitTrack_Pro.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitTrack_Pro.Services
{
    public class ChatService(IUnitOfWork unitOfWork ,UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager) : IChatService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        public async Task<ResponseModel<IEnumerable<ChatsViewModel>>> getChatsAsync(string currentUserId)
        {
            ResponseModel<IEnumerable<ChatsViewModel>> response = new ResponseModel<IEnumerable<ChatsViewModel>>();
            try
            {
                var adminRole = await _roleManager.FindByNameAsync("Admin");
           
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                var adminIds = adminUsers.Select(u => u.Id).ToHashSet();

                var chats = await _userManager.Users
                    .Where(u => !adminIds.Contains(u.Id))
                    .Select(u => new ChatsViewModel
                    {
                        FullName = u.FullName,
                        UserId = u.Id
                    })
                    .ToListAsync();


                foreach (var chat in chats)
                {
                    var user = await _userManager.FindByIdAsync(chat.UserId);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        chat.UserRole = roles.FirstOrDefault() ?? "Member";
                    }

                    Message message = await _unitOfWork.Messages
                        .GetAllAsync()
                        .Where(x => !x.IsDeleted && ((x.SenderId == chat.UserId || x.ReciverId == chat.UserId) &&
                        (x.SenderId == currentUserId || x.ReciverId == currentUserId)))
                          .OrderByDescending(m => m.CreatedAt)
                           .FirstOrDefaultAsync();
                    if(message != null)
                    {
                        chat.lastMessage = message.MessageContent;
                        chat.lastMessageSenderId = message.SenderId;
                        chat.lastMessageCreationDate = message.CreatedAt;
                    }
                }
                response.IsSuccess = true;
                response.Message = "Chats Loaded Successfully"; 
                response.Data = chats.OrderByDescending(x=>x.lastMessageCreationDate);
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        public async Task<ResponseModel<IEnumerable<MessageViewModel>>> getChatMessages(string currentUserId , string otherUserId)
        {
            ResponseModel<IEnumerable<MessageViewModel>> response = new ResponseModel<IEnumerable<MessageViewModel>>();

            try
            {
                IEnumerable<MessageViewModel> messages = await _unitOfWork.Messages
                    .GetAllAsync()
                    .Where(x => !x.IsDeleted && 
                    ((x.SenderId == currentUserId ||  x.ReciverId == currentUserId) && 
                    (x.SenderId ==  otherUserId || x.ReciverId == otherUserId)))
                    .Select(x => new MessageViewModel
                    {
                        Id = x.Id,
                        SenderId = x.SenderId,
                        SenderName = x.Sender.FullName,
                        Message = x.MessageContent,
                        SentAt = x.CreatedAt
                    } ) .ToListAsync();

                response.IsSuccess = true;
                response.Data = messages;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Data= null;
                return response;    
            }
        }

        public async Task<ResponseModel<IEnumerable<ChatsViewModel>>> getChatsV2Async(string currentUserId)
        {
            ResponseModel<IEnumerable<ChatsViewModel>> response = new ResponseModel<IEnumerable<ChatsViewModel>>();
            try
            {
                var adminRole = await _roleManager.FindByNameAsync("Admin");

                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                var adminIds = adminUsers.Select(u => u.Id).ToHashSet();

                var chats = await _userManager.Users
                    .Where(u => adminIds.Contains(u.Id))
                    .Select(u => new ChatsViewModel
                    {
                        FullName = u.FullName,
                        UserId = u.Id
                    })
                    .ToListAsync();


                foreach (var chat in chats)
                {
                    var user = await _userManager.FindByIdAsync(chat.UserId);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        chat.UserRole = roles.FirstOrDefault() ?? "Admin";
                    }

                    Message message = await _unitOfWork.Messages
                        .GetAllAsync()
                        .Where(x => !x.IsDeleted && ((x.SenderId == chat.UserId || x.ReciverId == chat.UserId) && 
                        (x.SenderId == currentUserId || x.ReciverId == currentUserId)))
                          .OrderByDescending(m => m.CreatedAt)
                           .FirstOrDefaultAsync();
                    if (message != null)
                    {
                        chat.lastMessage = message.MessageContent;
                        chat.lastMessageSenderId = message.SenderId;
                        chat.lastMessageCreationDate = message.CreatedAt;
                    }
                }
                response.IsSuccess = true;
                response.Message = "Chats Loaded Successfully";
                response.Data = chats.OrderByDescending(x => x.lastMessageCreationDate);
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }
        public async Task<ResponseModel<Message>>AddMessage(MessageRequestViewModel request)
        {
            ResponseModel<Message>response = new ResponseModel<Message>();

            try
            {
                Message message = new Message
                {
                    SenderId = request.SenderId,
                    ReciverId = request.ReciverId,
                    MessageContent = request.Message
                };
                await  _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.CompleteAsync();  
                response.IsSuccess = true;
                response.Data = message;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        public async Task<ResponseModel<bool>> UpdateMessageMessage(int MessageId,UpdateMessageViewModel request)
        {
            ResponseModel<bool> response = new ResponseModel<bool>();

            try
            {
                var message = await _unitOfWork.Messages.GetAllAsync()
                               .Where(x => x.Id == MessageId)
                               .FirstOrDefaultAsync();

                message.MessageContent = request.MesssageContent;
                _unitOfWork.Messages.Update(message);
                await _unitOfWork.CompleteAsync();
                response.IsSuccess = true;
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Data = false;
                return response;
            }
        }
        public async Task<ResponseModel<bool>> DeleteMessage(int MessageId)
        {
            ResponseModel<bool> response = new ResponseModel<bool>();
            try
            {
                var message = await _unitOfWork.Messages.GetAllAsync()
                               .Where(x => x.Id == MessageId)
                               .FirstOrDefaultAsync();
                message.IsDeleted = true;
                _unitOfWork.Messages.Update(message);
                await _unitOfWork.CompleteAsync();
                response.IsSuccess = true;
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Data = false;
                return response;
            }
        }

    }
}
