namespace FitTrack_Pro.ViewModels
{
    public class ChatsViewModel
    {
        public string UserId { get; set; }

        public string FullName { get; set; }

        public string? lastMessage { get; set; }

        public string? lastMessageSenderId { get; set; }

        public DateTime ? lastMessageCreationDate { get; set; }
    }
}
