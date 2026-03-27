using FitTrack_Pro.ViewModels;

namespace FitTrack_Pro.Interfaces
{
    /// <summary>
    /// Application-level gym class operations (business logic layer).
    /// </summary>
    public interface IGymClassService
    {
        Task<GymClassIndexViewModel> GetPagedGymClassesAsync(int page, int pageSize, string? search);
        Task<GymClassDetailsViewModel?> GetGymClassDetailsAsync(int id);
        Task<GymClassFormViewModel> GetCreateFormAsync();
        Task<GymClassFormViewModel?> GetEditFormAsync(int id);

        /// <returns>The newly created GymClass Id.</returns>
        Task<(bool Success, string? Error, int NewId)> CreateGymClassAsync(GymClassFormViewModel model);

        Task<(bool Success, string? Error)> UpdateGymClassAsync(GymClassFormViewModel model);
        Task<(bool Success, string? Error)> DeleteGymClassAsync(int id);

        // Weekly Schedule
        Task<WeeklyScheduleViewModel> GetWeeklyScheduleAsync(DateTime? startDate = null);

        // Member Assignment
        Task<GymClassAssignMemberViewModel?> GetAssignMemberFormAsync(int gymClassId);
        Task<(bool Success, string? Error)> AssignMemberAsync(GymClassAssignMemberViewModel model);
    }
}
