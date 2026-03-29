using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack_Pro.Models
{
    public class Message:BaseEntity
    {
        [Required]
        [ForeignKey("Sender")]
        public string SenderId { get; set; }
        [Required]
        [ForeignKey("Reciver")]
        public string ReciverId { get; set; }

        public ApplicationUser Sender { get; set; }

        public ApplicationUser Reciver { get; set; }

        public string MessageContent { get; set; }
        public bool IsSeen { get; set; }

        public DateTime? SeenAt { get; set; }


    }
}
