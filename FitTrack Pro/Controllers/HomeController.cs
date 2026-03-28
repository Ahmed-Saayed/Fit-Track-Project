using Common;
using FitTrack_Pro.Models;
using FitTrack_Pro.Services;
using FitTrack_Pro.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FitTrack_Pro.Controllers
{
	public class HomeController(IDashboardService _service) : Controller
	{
		[Authorize]
		public async Task<IActionResult> Index()
		{
			/*
			var today = DateTime.Today;
			var startOfMonth = new DateTime(today.Year, today.Month, 1);

			// 1. عدد المشتركين النشطين (اللي عندهم اشتراك شغال وماانتهيش)
			var activeMembersCount = await _context.MembersSubscription
				.Where(ms => ms.IsActive && ms.EndDate >= today && !ms.IsDeleted)
				.Select(ms => ms.MemberId)
				.Distinct()
				.CountAsync();

			// 2. إيرادات اليوم (مجموع المدفوعات اللي تمت النهاردة)
			var todaysRevenue = await _context.MembersSubscription
				.Where(ms => ms.CreatedAt.Date == today && !ms.IsDeleted)
				.SumAsync(ms => ms.PaidAmount);

			// 3. الاشتراكات اللي هتنتهي خلال 7 أيام
			var expiringSubs = await _context.MembersSubscription
				.Where(ms => ms.IsActive && ms.EndDate >= today && ms.EndDate <= today.AddDays(7) && !ms.IsDeleted)
				.CountAsync();

			// 4. كلاسات اليوم
			var classesToday = await _context.GymClasses
				.Where(c => c.ScheduleTime.Date == today && !c.IsDeleted)
				.CountAsync();

			// 5. المشتركين الجدد هذا الشهر
			var memberGrowth = await _context.Members
				.Where(m => m.CreatedAt >= startOfMonth && !m.IsDeleted)
				.CountAsync();

			// 6. آخر 5 عمليات حضور (Recent Activity)
			var recentAttendances = await _context.classAttendances
				.Include(ca => ca.Member)
				.Where(ca => !ca.IsDeleted)
				.OrderByDescending(ca => ca.AttendanceDate)
				.Take(5)
				.Select(ca => new ActivityLogDto
				{
					Time = ca.AttendanceDate.ToString("hh:mm tt"),
					MemberName = ca.Member != null ? ca.Member.FullName : "Unknown",
					MemberInitials = GetInitials(ca.Member != null ? ca.Member.FullName : "UN"),
					Status = "Checked-in",
					StatusColor = "success"
				})
				.ToListAsync();

			// 7. حساب نسبة الامتلاء للكلاسات اليوم (بشكل تقريبي)
			var todaysClassesWithCapacity = await _context.GymClasses
				.Include(c => c.Attendees)
				.Where(c => c.ScheduleTime.Date == today && !c.IsDeleted)
				.ToListAsync();

			int classFillRate = 0;
			if (todaysClassesWithCapacity.Any(c => c.MaxCapacity > 0))
			{
				var totalCapacity = todaysClassesWithCapacity.Sum(c => c.MaxCapacity);
				var totalAttendees = todaysClassesWithCapacity.Sum(c => c.Attendees.Count(a => !a.IsDeleted));
				classFillRate = (int)Math.Round((double)totalAttendees / totalCapacity * 100);
			}

			// تجميع الداتا في الـ ViewModel
			var viewModel = new DashboardViewModel
			{
				TotalActiveMembers = activeMembersCount,
				TodaysRevenue = todaysRevenue,
				ExpiringSubscriptions = expiringSubs,
				ClassesToday = classesToday,
				MemberGrowth = memberGrowth,
				ClassFillRate = classFillRate,
				GymCapacityPercentage = classFillRate > 0 ? classFillRate : 35, // رقم افتراضي لو مفيش كلاسات
				RecentActivities = recentAttendances
			};
			*/
            var viewModel = await _service.GetDashboardDataAsync();
            return View(viewModel);
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
