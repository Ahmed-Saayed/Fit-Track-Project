using Common;
using FitTrack_Pro.Models;

namespace FitTrack_Pro.Interfaces
{
    /// <summary>
    /// Trainer-specific repository extending the generic one
    /// with domain-specific query methods.
    /// </summary>
    public interface ITrainerRepository : IGenericRepository<Trainer>
    {
        /// <summary>Returns a trainer including their assigned classes.</summary>
        Task<Trainer?> GetWithClassesAsync(int trainerId);

        /// <summary>Full-text search on name, phone, or specialty.</summary>
        Task<IEnumerable<Trainer>> SearchAsync(string keyword);
    }
}
