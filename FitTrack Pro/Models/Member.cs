using System.ComponentModel.DataAnnotations;

namespace FitTrack_Pro.Models
{
	public class Member : BaseEntity
	{
		[Required, StringLength(100)]
		public string FullName { get; set; } = string.Empty;

		[Required, StringLength(20)]
		public string PhoneNumber { get; set; } = string.Empty;

		public DateTime BirthDate { get; set; }

		[StringLength(10)]
		public string Gender { get; set; } = string.Empty;

		[Required, StringLength(50)]
		public string? Barcode { get; set; } = string.Empty;

		public string? MedicalNotes { get; set; }
		//public bool IsActive { get; set; } = true;
		public string? UserId { get; set; }
		public virtual ApplicationUser? UserAccount { get; set; }
		// Relationships
		public  ICollection<MemberSubscription> Subscriptions { get; set; } 
		public  ICollection<ClassAttendance> Attendances { get; set; } 
	}
}
