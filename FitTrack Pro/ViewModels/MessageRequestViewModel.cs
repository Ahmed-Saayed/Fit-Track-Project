using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack_Pro.ViewModels
{
    public class MessageRequestViewModel
    {
        public string SenderId { get; set; }
        public string ReciverId { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
