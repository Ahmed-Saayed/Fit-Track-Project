namespace FitTrack_Pro.ViewModels
{
	public class DashboardViewModel
	{
		// Top 4 Cards
		public int TotalActiveMembers { get; set; }
		public decimal TodaysRevenue { get; set; }
		public int ExpiringSubscriptions { get; set; }
		public int ClassesToday { get; set; }

		// Gym Capacity & Growth
		public int GymCapacityPercentage { get; set; }
		public int MemberGrowth { get; set; }
		public int ClassFillRate { get; set; }

		// Recent Activity List
		public List<ActivityLogDto> RecentActivities { get; set; } = new List<ActivityLogDto>();
	}

	
}