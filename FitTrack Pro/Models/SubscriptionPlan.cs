using System.ComponentModel.DataAnnotations;

namespace FitTrack_Pro.Models
{
		public class SubscriptionPlan : BaseEntity
		{
			[Required, StringLength(50)]
			public string Name { get; set; } = string.Empty;
			public int DurationInDays { get; set; }
			public decimal Price { get; set; }
			public string? Description { get; set; }
		}
}
