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
				members = members.Skip((page - 1) * pageSize).Take(pageSize);
			}
			else
			{
				total = await memberRepo.CountAsync(m => !m.IsDeleted);
				members = await memberRepo.GetAllAsync()
					.Where(m => !m.IsDeleted)
					.Include(m => m.Subscriptions)
						.ThenInclude(s => s.SubscriptionPlan)
					.Include(m => m.Attendances)
						.ThenInclude(a => a.GymClass)
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
	var allSubs = await uow.MemberSubscriptions.GetAllAsync()
				.Include(s => s.SubscriptionPlan)
				.Where(s => s.MemberId == id && !s.IsDeleted)
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
				ActiveSubscription = allSubs
					.Where(s => s.IsActive && s.EndDate >= DateTime.Today)
					.Select(MapSubscription)
					.FirstOrDefault(),
				SubscriptionHistory = allSubs.Select(MapSubscription),
				JoinedClasses = member.Attendances
					.Select(a => a.GymClass?.Name ?? "Admin Class")
					.Distinct()
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

			var user = new ApplicationUser
			{
				UserName = model.UserName ?? model.Barcode,
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

			if (!await roleManager.RoleExistsAsync("Member"))
				await roleManager.CreateAsync(new IdentityRole("Member"));

			await userManager.AddToRoleAsync(user, "Member");

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
		//  DASHBOARD STATS (✅ Optimized Version)
		// ────────────────────────────────────────────────────────────
		public async Task<MemberDashboardStatsViewModel> GetDashboardStatsAsync()
		{
			var today = DateTime.Today;
			var thisMonthStart = new DateTime(today.Year, today.Month, 1);

			var totalMembers = await memberRepo.GetAllAsync().CountAsync(m => !m.IsDeleted);

			var activeMembers = await uow.MemberSubscriptions.GetAllAsync()
				.Where(s => s.IsActive && s.EndDate >= today && !s.IsDeleted)
				.Select(s => s.MemberId)
				.Distinct()
				.CountAsync();

			var expiringIn7Days = await uow.MemberSubscriptions.GetAllAsync()
				.Where(s => s.IsActive && s.EndDate >= today && s.EndDate <= today.AddDays(7) && !s.IsDeleted)
				.Select(s => s.MemberId)
				.Distinct()
				.CountAsync();

			var newThisMonth = await memberRepo.GetAllAsync()
				.CountAsync(m => !m.IsDeleted && m.CreatedAt >= thisMonthStart);

			return new MemberDashboardStatsViewModel
			{
				TotalMembers = totalMembers,
				ActiveMembers = activeMembers,
				ExpiredMembers = totalMembers - activeMembers,
				ExpiringIn7Days = expiringIn7Days,
				NewThisMonth = newThisMonth
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
				.Where(p => !p.IsDeleted)
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

			// إيقاف الباقات القديمة النشطة
			var currentActive = await uow.MemberSubscriptions.GetAllAsync()
				.Where(s => s.MemberId == model.MemberId && s.IsActive && !s.IsDeleted)
				.ToListAsync();

			foreach (var sub in currentActive)
			{
				sub.IsActive = false;
				uow.MemberSubscriptions.Update(sub);
			}

			// ✅ إنشاء الاشتراك وتعيين المدفوع للدفعة الأولى (وليس السعر الكامل)
			var newSub = new MemberSubscription
			{
				MemberId = model.MemberId,
				SubscriptionPlanId = plan.Id,
				StartDate = model.StartDate,
				EndDate = model.StartDate.AddDays(plan.DurationInDays),
				PaidAmount = model.InitialPayment,
				IsActive = true
			};

			await uow.MemberSubscriptions.AddAsync(newSub);

			// ✅ تسجيل الدفعة الأولى في جدول الدفعات
			if (model.InitialPayment > 0)
			{
				var payment = new SubscriptionPayment
				{
					MemberSubscription = newSub,
					Amount = model.InitialPayment,
					PaymentMethod = model.PaymentMethod,
					PaymentDate = DateTime.Now,
					Notes = "Initial payment upon plan assignment"
				};
				await uow.SubscriptionPayments.AddAsync(payment);
			}

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
				.FirstOrDefaultAsync(m => m.UserId == userId && !m.IsDeleted);

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
		//  INSTALLMENT PAYMENT
		// ────────────────────────────────────────────────────────────
		public async Task<(bool Success, string? Error, int MemberId)> AddInstallmentPaymentAsync(int subscriptionId, decimal amount, string paymentMethod)
		{
			var subscription = await uow.MemberSubscriptions.GetByIdAsync(subscriptionId);

			if (subscription == null || subscription.IsDeleted)
				return (false, "Subscription not found.", 0);

			var payment = new SubscriptionPayment
			{
				MemberSubscriptionId = subscriptionId,
				Amount = amount,
				PaymentMethod = paymentMethod,
				PaymentDate = DateTime.Now,
				Notes = "Installment Payment"
			};

			await uow.SubscriptionPayments.AddAsync(payment);

			subscription.PaidAmount += amount;
			uow.MemberSubscriptions.Update(subscription);

			await uow.CompleteAsync();

			return (true, null, subscription.MemberId);
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
				JoinedClasses = m.Attendances?
					.Select(a => a.GymClass?.Name ?? "Admin Class")
					.Distinct().ToList() ?? [],
				Status = status,
				CreatedAt = m.CreatedAt
			};
		}

		private static MemberSubscriptionViewModel MapSubscription(MemberSubscription s) => new()
		{
			Id = s.Id,
			PlanName = s.SubscriptionPlan?.Name ?? "—",
			PlanPrice = s.SubscriptionPlan?.Price ?? 0,
			StartDate = s.StartDate,
			EndDate = s.EndDate,
			PaidAmount = s.PaidAmount,
			IsActive = s.IsActive
		};
	}
}