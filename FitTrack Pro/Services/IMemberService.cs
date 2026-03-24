using FitTrack_Pro.ViewModels;

namespace FitTrack_Pro.Interfaces
{
    /// <summary>
    /// Application-level member operations (business logic layer).
    /// </summary>
    public interface IMemberService
    {
        Task<MemberIndexViewModel> GetPagedMembersAsync(int page, int pageSize, string? search);
        Task<MemberDetailsViewModel?> GetMemberDetailsAsync(int id);
        Task<MemberFormViewModel> GetCreateFormAsync();
        Task<MemberFormViewModel?> GetEditFormAsync(int id);

        /// <returns>The newly created member's Id.</returns>
        Task<(bool Success, string? Error, int NewId)> CreateMemberAsync(MemberFormViewModel model);

        Task<(bool Success, string? Error)> UpdateMemberAsync(MemberFormViewModel model);
        Task<(bool Success, string? Error)> DeleteMemberAsync(int id);

        Task<MemberDashboardStatsViewModel> GetDashboardStatsAsync();
    }
}