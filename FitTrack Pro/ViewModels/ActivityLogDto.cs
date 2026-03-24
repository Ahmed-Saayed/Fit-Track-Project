namespace FitTrack_Pro.ViewModels
{
	public class ActivityLogDto
	{
		public string Time { get; set; } = string.Empty;
		public string MemberInitials { get; set; } = string.Empty;
		public string MemberName { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty; // Checked-in, Booked Class, Access Denied
		public string StatusColor { get; set; } = string.Empty; // success, primary, danger
	}
}
