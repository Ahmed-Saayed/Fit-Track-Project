using FitTrack_Pro.ViewModels;
using Common;

namespace FitTrack_Pro.Services
{
	public class DashboardService(IUnitOfWork uow) : IDashboardService
	{
		public async Task<DashboardViewModel> GetDashboardDataAsync()
		{
			var today = DateTime.Today;
			var startOfMonth = new DateTime(today.Year, today.Month, 1);

			// جلب الداتا الأساسية (بنفترض إن GetAllAsync بترجع الداتا المتاحة)
			// ملاحظة: تأكد من اسم ClassAttendaces في الـ UoW عندك، أنا كتبتها زي ما بعتهالي
			var allSubs =  uow.MemberSubscriptions.GetAllAsync();
			var allClasses =  uow.GymClasses.GetAllAsync();
			var allMembers = uow.Members.GetAllAsync();
			var allAttendances = uow.ClassAttendaces.GetAllAsync();

			// 1. عدد المشتركين النشطين
			var activeMembersCount = allSubs
				.Where(ms => ms.IsActive && ms.EndDate >= today && !ms.IsDeleted)
				.Select(ms => ms.MemberId)
				.Distinct()
				.Count();

			// 2. إيرادات اليوم
			var todaysRevenue = allSubs
				.Where(ms => ms.CreatedAt.Date == today && !ms.IsDeleted)
				.Sum(ms => ms.PaidAmount);

			// 3. الاشتراكات اللي هتنتهي خلال 7 أيام
			var expiringSubs = allSubs
				.Where(ms => ms.IsActive && ms.EndDate >= today && ms.EndDate <= today.AddDays(7) && !ms.IsDeleted)
				.Count();

			// 4. كلاسات اليوم
			var classesToday = allClasses
				.Where(c => c.ScheduleTime.Date == today && !c.IsDeleted)
				.Count();

			// 5. المشتركين الجدد هذا الشهر
			var memberGrowth = allMembers
				.Where(m => m.CreatedAt >= startOfMonth && !m.IsDeleted)
				.Count();

			
			var recentAttendances = allAttendances
				.Where(ca => !ca.IsDeleted)
				.OrderByDescending(ca => ca.AttendanceDate)
				.Take(5)
				.AsEnumerable() // <--- السطر ده هو مفتاح الحل (بيفصل بين الـ Database والـ Memory)
				.Select(ca =>
				{
					var member = allMembers.FirstOrDefault(m => m.Id == ca.MemberId);
					var fullName = member?.FullName ?? "Unknown";

					return new ActivityLogDto
					{
						Time = ca.AttendanceDate.ToString("hh:mm tt"),
						MemberName = fullName,
						MemberInitials = GetInitials(fullName),
						Status = "Checked-in",
						StatusColor = "success"
					};
				})
				.ToList();

			// 7. حساب نسبة الامتلاء للكلاسات اليوم
			var todaysClassesList = allClasses
				.Where(c => c.ScheduleTime.Date == today && !c.IsDeleted)
				.ToList();

			int classFillRate = 0;
			if (todaysClassesList.Any(c => c.MaxCapacity > 0))
			{
				var totalCapacity = todaysClassesList.Sum(c => c.MaxCapacity);

				// بنحسب عدد الحضور الفعلي لكلاسات اليوم من جدول الحضور لضمان الدقة
				var todaysClassesIds = todaysClassesList.Select(c => c.Id).ToList();
				var totalAttendees = allAttendances.Count(a => todaysClassesIds.Contains(a.GymClassId));

				classFillRate = (int)Math.Round((double)totalAttendees / totalCapacity * 100);
			}

			return new DashboardViewModel
			{
				TotalActiveMembers = activeMembersCount,
				TodaysRevenue = todaysRevenue,
				ExpiringSubscriptions = expiringSubs,
				ClassesToday = classesToday,
				MemberGrowth = memberGrowth,
				ClassFillRate = classFillRate,
				GymCapacityPercentage = classFillRate > 0 ? classFillRate : 35,
				RecentActivities = recentAttendances
			};
		}

		private static string GetInitials(string fullName)
		{
			if (string.IsNullOrWhiteSpace(fullName)) return "GU";
			var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
			return $"{parts[0][0]}{parts[1][0]}".ToUpper();
		}
	}
}