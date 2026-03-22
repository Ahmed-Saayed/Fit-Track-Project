using System.ComponentModel.DataAnnotations;

namespace FitTrack_Pro.Models
{
	public abstract class BaseEntity
	{
		[Key]
		public int Id { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public string? CreatedBy { get; set; }

		public bool IsDeleted { get; set; } = false;
	}
}
