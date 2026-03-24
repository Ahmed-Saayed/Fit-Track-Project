using Common;
using FitTrack_Pro.Models;
using System.Linq.Expressions;

namespace FitTrack_Pro.Interfaces
{
    /// <summary>
    /// Member-specific repository extending the generic one
    /// with domain-specific query methods.
    /// </summary>
    public interface IMemberRepository : IGenericRepository<Member>
    {
        /// <summary>Returns a member including their active subscription and plan.</summary>
        Task<Member?> GetWithActiveSubscriptionAsync(int memberId);

        /// <summary>Returns all members whose subscription expires within the given days.</summary>
        Task<IEnumerable<Member>> GetExpiringMembersAsync(int withinDays);

        /// <summary>Full-text search on name, phone, or barcode.</summary>
        Task<IEnumerable<Member>> SearchAsync(string keyword);

        /// <summary>Checks whether a barcode already exists (for uniqueness validation).</summary>
        Task<bool> BarcodeExistsAsync(string barcode, int? excludeMemberId = null);
    }
}