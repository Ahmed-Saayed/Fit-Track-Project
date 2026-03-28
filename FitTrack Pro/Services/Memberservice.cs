using Common;
using FitTrack_Pro.Interfaces;
using FitTrack_Pro.Models;
using FitTrack_Pro.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitTrack_Pro.Services
{
	public class MemberService(
		IMemberRepository memberRepo,
		IUnitOfWork uow,
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole> roleManager) : IMemberService
	{
		// ────────────────────────────────────────────────────────────
		//  PAGED LIST  (with optional search)
		// ────────────────────────────────────────────────────────────
		public async Task<MemberIndexViewModel> GetPagedMembersAsync(
			int page, int pageSize, string? search)
		{
			IEnumerable<Member> members;
			int total;

			if (!string.IsNullOrWhiteSpace(search))
			{
				members = await memberRepo.SearchAsync(search);
				total = members.Count();
				// Apply paging manually on search results
				members = members
					.Skip((page - 1) * pageSize)
					.Take(pageSize);
			}
			else
			{
				total = await memberRepo.CountAsync(m => !m.IsDeleted);
				// We need to include Subscriptions and SubscriptionPlan for the Index view
				members = await memberRepo.GetAllAsync()
					.Where(m => !m.IsDeleted)
					.Include(m => m.Subscriptions)
						.ThenInclude(s => s.SubscriptionPlan)
					.OrderByDescending(m => m.CreatedAt)
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToListAsync();
			}

			return new MemberIndexViewModel
			{
				Members = members.Select(MapToRow),
				CurrentPage = page,
				PageSize = pageSize,
				TotalCount = total,
				TotalPages = (int)Math.Ceiling(total / (double)pageSize),
				SearchQuery = search
			};
		}

		// ────────────────────────────────────────────────────────────
		//  DETAILS
		// ────────────────────────────────────────────────────────────
		public async Task<MemberDetailsViewModel?> GetMemberDetailsAsync(int id)
		{
			var member = await memberRepo.GetWithActiveSubscriptionAsync(id);
			if (member is null) return null;

			// also load full subscription history
			var allSubs = await uow.MemberSubscriptions
				.GetAllAsync()
				.Where(s => s.MemberId == id)
				.OrderByDescending(s => s.StartDate)
				.ToListAsync();

			var vm = new MemberDetailsViewModel
			{
				Id = member.Id,
				FullName = member.FullName,
				PhoneNumber = member.PhoneNumber,
				BirthDate = member.BirthDate,
				Gender = member.Gender,
				Barcode = member.Barcode,
				MedicalNotes = member.MedicalNotes,
				CreatedAt = member.CreatedAt,
				ActiveSubscription = member.Subscriptions
					.Where(s => s.IsActive && s.EndDate >= DateTime.Today)
					.Select(MapSubscription)
					.FirstOrDefault(),
				SubscriptionHistory = allSubs.Select(MapSubscription)
			};

			return vm;
		}

		// ────────────────────────────────────────────────────────────
		//  FORM  (for both Create and Edit)
		// ────────────────────────────────────────────────────────────
		public Task<MemberFormViewModel> GetCreateFormAsync()
			=> Task.FromResult(new MemberFormViewModel());

		public async Task<MemberFormViewModel?> GetEditFormAsync(int id)
		{
			var member = await memberRepo.GetByIdAsync(id);
			if (member is null || member.IsDeleted) return null;

			return new MemberFormViewModel
			{
				Id = member.Id,
				FullName = member.FullName,
				PhoneNumber = member.PhoneNumber,
				BirthDate = member.BirthDate,
				Gender = member.Gender,
				Barcode = member.Barcode!,
				MedicalNotes = member.MedicalNotes
			};
		}

		// ────────────────────────────────────────────────────────────
		//  CREATE
		// ────────────────────────────────────────────────────────────
		public async Task<(bool Success, string? Error, int NewId)> CreateMemberAsync(
			MemberFormViewModel model)
		{
			if (await memberRepo.BarcodeExistsAsync(model.Barcode))
				return (false, "Barcode is already in use by another member.", 0);

			// 1. Create Identity User
			var user = new ApplicationUser
			{
				UserName = model.UserName ?? model.Barcode, // Fallback to barcode if email missing
				Email = model.UserName,
				FullName = model.FullName,
				PhoneNumber = model.PhoneNumber
			};

			var userResult = await userManager.CreateAsync(user, model.Password ?? "Pass123!");
			if (!userResult.Succeeded)
			{
				var firstErr = userResult.Errors.FirstOrDefault()?.Description ?? "User creation failed.";
				return (false, firstErr, 0);
			}

			// 2. Assign "Member" Role
			if (!await roleManager.RoleExistsAsync("Member"))
			{
				await roleManager.CreateAsync(new IdentityRole("Member"));
			}
			await userManager.AddToRoleAsync(user, "Member");

			// 3. Create Member Record
			var member = new Member
			{
				FullName = model.FullName.Trim(),
				PhoneNumber = model.PhoneNumber.Trim(),
				BirthDate = model.BirthDate,
				Gender = model.Gender,
				Barcode = model.Barcode.Trim(),
				MedicalNotes = model.MedicalNotes?.Trim(),
				CreatedAt = DateTime.Now,
				UserId = user.Id
			};

			await memberRepo.AddAsync(member);
			await uow.CompleteAsync();

			return (true, null, member.Id);
		}

		// ────────────────────────────────────────────────────────────
		//  UPDATE
		// ────────────────────────────────────────────────────────────
		public async Task<(bool Success, string? Error)> UpdateMemberAsync(
			MemberFormViewModel model)
		{
			var member = await memberRepo.GetByIdAsync(model.Id);
			if (member is null || member.IsDeleted)
				return (false, "Member not found.");

			if (await memberRepo.BarcodeExistsAsync(model.Barcode, model.Id))
				return (false, "Barcode is already in use by another member.");

			member.FullName = model.FullName.Trim();
			member.PhoneNumber = model.PhoneNumber.Trim();
			member.BirthDate = model.BirthDate;
			member.Gender = model.Gender;
			member.Barcode = model.Barcode.Trim();
			member.MedicalNotes = model.MedicalNotes?.Trim();

			memberRepo.Update(member);
			await uow.CompleteAsync();

			return (true, null);
		}

		// ────────────────────────────────────────────────────────────
		//  SOFT DELETE
		// ────────────────────────────────────────────────────────────
		public async Task<(bool Success, string? Error)> DeleteMemberAsync(int id)
		{
			var member = await memberRepo.GetByIdAsync(id);
			if (member is null || member.IsDeleted)
				return (false, "Member not found.");

			member.IsDeleted = true;
			memberRepo.Update(member);
			await uow.CompleteAsync();

			return (true, null);
		}

		// ────────────────────────────────────────────────────────────
		//  DASHBOARD STATS
		// ────────────────────────────────────────────────────────────
		public async Task<MemberDashboardStatsViewModel> GetDashboardStatsAsync()
		{
			var allMembers = await memberRepo.GetAllAsync()
				.Where(m => !m.IsDeleted)
				.ToListAsync();

			var allSubs = await uow.MemberSubscriptions.GetAllAsync().ToListAsync();

			var activeMemberIds = allSubs
				.Where(s => s.IsActive && s.EndDate >= DateTime.Today)
				.Select(s => s.MemberId)
				.Distinct()
				.ToHashSet();

			var expiringIds = allSubs
				.Where(s => s.IsActive &&
							s.EndDate >= DateTime.Today &&
							s.EndDate <= DateTime.Today.AddDays(7))
				.Select(s => s.MemberId)
				.Distinct()
				.ToHashSet();

			var thisMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

			return new MemberDashboardStatsViewModel
			{
				TotalMembers = allMembers.Count,
				ActiveMembers = activeMemberIds.Count,
				ExpiredMembers = allMembers.Count(m => !activeMemberIds.Contains(m.Id)),
				ExpiringIn7Days = expiringIds.Count,
				NewThisMonth = allMembers.Count(m => m.CreatedAt >= thisMonthStart)
			};
		}

		// ────────────────────────────────────────────────────────────
		//  NEW PLAN & DASHBOARD METHODS
		// ────────────────────────────────────────────────────────────
		public async Task<AssignPlanViewModel?> GetAssignPlanFormAsync(int memberId)
		{
			var member = await memberRepo.GetByIdAsync(memberId);
			if (member is null || member.IsDeleted) return null;

			var activeSub = await uow.MemberSubscriptions.GetAllAsync()
				.Include(s => s.SubscriptionPlan)
				.FirstOrDefaultAsync(s => s.MemberId == memberId && s.IsActive && s.EndDate >= DateTime.Today);

			var plans = await uow.SubscriptionPlans.GetAllAsync()
				.Select(p => new PlanResponseViewModel
				{
					Id = p.Id,
					Name = p.Name,
					DurationInDays = p.DurationInDays,
					Price = p.Price,
					Description = p.Description
				}).ToListAsync();

			return new AssignPlanViewModel
			{
				MemberId = member.Id,
				MemberName = member.FullName,
				CurrentPlanName = activeSub?.SubscriptionPlan?.Name,
				CurrentPlanEndDate = activeSub?.EndDate,
				AvailablePlans = plans,
				StartDate = DateTime.Today
			};
		}

		public async Task<(bool Success, string? Error)> AssignPlanAsync(AssignPlanViewModel model)
		{
			var plan = await uow.SubscriptionPlans.GetByIdAsync(model.SelectedPlanId);
			if (plan is null) return (false, "Subscription plan not found.");

			// Optionally deactivate existing active plans
			var currentActive = await uow.MemberSubscriptions.GetAllAsync()
				.Where(s => s.MemberId == model.MemberId && s.IsActive)
				.ToListAsync();

			foreach (var sub in currentActive)
			{
				sub.IsActive = false;
			}

			var newSub = new MemberSubscription
			{
				MemberId = model.MemberId,
				SubscriptionPlanId = plan.Id,
				StartDate = model.StartDate,
				EndDate = model.StartDate.AddDays(plan.DurationInDays),
				PaidAmount = plan.Price,
				IsActive = true
			};

			await uow.MemberSubscriptions.AddAsync(newSub);
			await uow.CompleteAsync();

			return (true, null);
		}

		public async Task<(bool Success, string? Error)> DeleteSubscriptionAsync(int subscriptionId)
		{
			var deleted = await uow.MemberSubscriptions.DeleteAsync(subscriptionId);
			if (!deleted) return (false, "Subscription not found.");

			await uow.CompleteAsync();
			return (true, null);
		}

		public async Task<MemberDashboardViewModel?> GetMemberDashboardAsync(string userId)
		{
			var member = await memberRepo.GetAllAsync()
				.Include(m => m.Subscriptions).ThenInclude(s => s.SubscriptionPlan)
				.Include(m => m.Attendances).ThenInclude(a => a.GymClass).ThenInclude(c => c.Trainer)
				.FirstOrDefaultAsync(m => m.UserId == userId);

			if (member is null) return null;

			return new MemberDashboardViewModel
			{
				MemberId = member.Id,
				FullName = member.FullName,
				Barcode = member.Barcode ?? "",
				PhoneNumber = member.PhoneNumber,
				Age = (int)((DateTime.Today - member.BirthDate).TotalDays / 365.25),
				ActiveSubscription = member.Subscriptions
					.Where(s => s.IsActive && s.EndDate >= DateTime.Today)
					.OrderByDescending(s => s.EndDate)
					.Select(MapSubscription)
					.FirstOrDefault(),
				AttendedClasses = member.Attendances
					.OrderByDescending(a => a.AttendanceDate)
					.Take(10)
					.Select(a => new AttendedClassViewModel
					{
						ClassName = a.GymClass?.Name ?? "Admin Class",
						TrainerName = a.GymClass?.Trainer?.FullName ?? "Staff",
						AttendanceDate = a.AttendanceDate,
						DurationInMinutes = a.GymClass?.DurationInMinutes ?? 0
					})
			};
		}

		// ────────────────────────────────────────────────────────────
		//  PRIVATE MAPPERS
		// ────────────────────────────────────────────────────────────
		private static MemberRowViewModel MapToRow(Member m)
		{
			var activeSub = m.Subscriptions?
				.FirstOrDefault(s => s.IsActive && s.EndDate >= DateTime.Today);

			var status = activeSub is not null
				? MemberStatus.Active
				: m.Subscriptions?.Any() == true
					? MemberStatus.Expired
					: MemberStatus.NeverSubscribed;

			return new MemberRowViewModel
			{
				Id = m.Id,
				FullName = m.FullName,
				PhoneNumber = m.PhoneNumber,
				Gender = m.Gender,
				Barcode = m.Barcode,
				Age = (int)((DateTime.Today - m.BirthDate).TotalDays / 365.25),
				ActivePlanName = activeSub?.SubscriptionPlan?.Name,
				SubscriptionEnd = activeSub?.EndDate,
				Status = status,
				CreatedAt = m.CreatedAt
			};
		}

		private static MemberSubscriptionViewModel MapSubscription(MemberSubscription s) => new()
		{
			Id = s.Id,
			PlanName = s.SubscriptionPlan?.Name ?? "—",
			StartDate = s.StartDate,
			EndDate = s.EndDate,
			PaidAmount = s.PaidAmount,
			IsActive = s.IsActive
		};
	}
}