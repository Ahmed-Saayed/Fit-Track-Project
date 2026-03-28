using FitTrack_Pro.ViewModels;

namespace FitTrack_Pro.Interfaces
{
    /// <summary>
    /// Application-level trainer operations (business logic layer).
    /// </summary>
    public interface ITrainerService
    {
        Task<TrainerIndexViewModel> GetPagedTrainersAsync(int page, int pageSize, string? search);
        Task<TrainerDetailsViewModel?> GetTrainerDetailsAsync(int id);
        Task<TrainerFormViewModel> GetCreateFormAsync();
        Task<TrainerFormViewModel?> GetEditFormAsync(int id);

        /// <returns>The newly created trainer's Id.</returns>
        Task<(bool Success, string? Error, int NewId)> CreateTrainerAsync(TrainerFormViewModel model);

        Task<(bool Success, string? Error)> UpdateTrainerAsync(TrainerFormViewModel model);
        Task<(bool Success, string? Error)> DeleteTrainerAsync(int id);
		//GetTrainerProfileByUserIdAsync
		//Task<List<TrainerSelectItemViewModel>> GetTrainerProfileByUserIdAsync();
	}
}
