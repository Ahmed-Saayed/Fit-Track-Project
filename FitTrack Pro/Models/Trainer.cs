	using System.ComponentModel.DataAnnotations;

	namespace FitTrack_Pro.Models
	{
		public class Trainer : BaseEntity
		{
			[Required, StringLength(100)]
			public string FullName { get; set; } = string.Empty;
			public string PhoneNumber { get; set; } = string.Empty;
			public string Specialty { get; set; } = string.Empty;
			public decimal SalaryOrPercentage { get; set; }
		public string? UserId { get; set; }
		public virtual ApplicationUser? UserAccount { get; set; }
		public  ICollection<GymClass> Classes { get; set; } 
		}
	}
