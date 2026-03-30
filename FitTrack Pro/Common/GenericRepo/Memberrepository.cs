using Common;
using FitTrack_Pro.Interfaces;
using FitTrack_Pro.Models;
using Microsoft.EntityFrameworkCore;

namespace FitTrack_Pro.Repositories
{
	public class MemberRepository(ApplicationDbContext context)
		: GenericRepository<Member>(context), IMemberRepository
	{
		private readonly ApplicationDbContext _db = context;

		// ──────────────────────────────────────────────────────────────
		//  Get a single member with their ACTIVE subscription + plan
		// ──────────────────────────────────────────────────────────────
		public async Task<Member?> GetWithActiveSubscriptionAsync(int memberId)
		{
			return await _db.Members
				.Where(m => m.Id == memberId && !m.IsDeleted)
				.Include(m => m.Subscriptions
					.Where(s => s.IsActive && s.EndDate >= DateTime.Today))
				.ThenInclude(s => s.SubscriptionPlan)
				.Include(m => m.Attendances)
					.ThenInclude(a => a.GymClass)
				.FirstOrDefaultAsync();
		}

		// ──────────────────────────────────────────────────────────────
		//  Members whose subscription expires within N days
		// ──────────────────────────────────────────────────────────────
		public async Task<IEnumerable<Member>> GetExpiringMembersAsync(int withinDays)
		{
			var cutoff = DateTime.Today.AddDays(withinDays);

			return await _db.Members
				.Where(m => !m.IsDeleted &&
							m.Subscriptions.Any(s =>
								s.IsActive &&
								s.EndDate >= DateTime.Today &&
								s.EndDate <= cutoff))
				.Include(m => m.Subscriptions
					.Where(s => s.IsActive && s.EndDate <= cutoff))
				.ThenInclude(s => s.SubscriptionPlan)
				.OrderBy(m => m.Subscriptions
					.Where(s => s.IsActive).Min(s => s.EndDate))
				.ToListAsync();
		}

		// ──────────────────────────────────────────────────────────────
		//  Full-text search (name / phone / barcode)
		// ──────────────────────────────────────────────────────────────
		public async Task<IEnumerable<Member>> SearchAsync(string keyword)
		{
			var kw = keyword.Trim().ToLower();

			return await _db.Members
				.Where(m => !m.IsDeleted &&
							(m.FullName.ToLower().Contains(kw) ||
							 m.PhoneNumber.Contains(kw) ||
							 (m.Barcode != null && m.Barcode.Contains(kw))))
				.Include(m => m.Subscriptions
					.Where(s => s.IsActive))
				.ThenInclude(s => s.SubscriptionPlan)
				.Include(m => m.Attendances)
					.ThenInclude(a => a.GymClass)
				.OrderBy(m => m.FullName)
				.Take(50)
				.ToListAsync();
		}

		// ──────────────────────────────────────────────────────────────
		//  Barcode uniqueness check
		// ──────────────────────────────────────────────────────────────
		public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeMemberId = null)
		{
			return await _db.Members
				.AnyAsync(m => !m.IsDeleted &&
							   m.Barcode == barcode &&
							   (!excludeMemberId.HasValue || m.Id != excludeMemberId.Value));
		}
	}
}