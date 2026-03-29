using FitTrack_Pro.Common;
using FitTrack_Pro.Models;
using FitTrack_Pro.ViewModels;

namespace FitTrack_Pro.Services
{
    public interface IChatService
    {
        Task<ResponseModel<IEnumerable<ChatsViewModel>>> getChatsAsync(string currentUserId);
        Task<ResponseModel<IEnumerable<MessageViewModel>>> getChatMessages(string currentUserId, string otherUserId);

        Task<ResponseModel<Message>> AddMessage(MessageRequestViewModel request);

        Task<ResponseModel<IEnumerable<ChatsViewModel>>> getChatsV2Async(string currentUserId);

        Task<ResponseModel<bool>> UpdateMessageMessage(int MessageId, UpdateMessageViewModel request);

        Task<ResponseModel<bool>> DeleteMessage(int MessageId);
    }
}
