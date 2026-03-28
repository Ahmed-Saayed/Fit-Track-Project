using FitTrack_Pro.ViewModels;

namespace FitTrack_Pro.Services
{
	public interface IDashboardService
	{
		Task<DashboardViewModel> GetDashboardDataAsync();
	}
}
