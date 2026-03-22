using System.ComponentModel.DataAnnotations;

namespace FitTrack_Pro.Models
{
	
	public class  MemberVisit :	BaseEntity
	{
		public decimal Height { get; set; }
		public decimal Weight { get; set; }
		public decimal BMI { get; set; }
		public string? Notes { get; set; }
		public ClassAttendance ?ClassAttendance { get; set; }
	}
}
