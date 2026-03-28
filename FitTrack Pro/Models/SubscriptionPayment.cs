using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack_Pro.Models
{
	public class SubscriptionPayment : BaseEntity
	{
		public int MemberSubscriptionId { get; set; }
		public MemberSubscription? MemberSubscription { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; } 

		public DateTime PaymentDate { get; set; } = DateTime.Now;

		public string? PaymentMethod { get; set; } 

		public string? Notes { get; set; }
	}
}