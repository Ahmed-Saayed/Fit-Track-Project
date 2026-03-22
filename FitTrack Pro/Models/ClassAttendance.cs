namespace FitTrack_Pro.Models
{
	public class ClassAttendance : BaseEntity
	{
		public int GymClassId { get; set; }
		public  GymClass? GymClass { get; set; }

		public int MemberId { get; set; }
		public  Member? Member { get; set; }

		public int MemberVisitId { get; set; }
		public MemberVisit ?MemberVisit { get; set; }
		public DateTime AttendanceDate { get; set; } = DateTime.Now;

	}
}
