using FitTrack_Pro.Interfaces;
using FitTrack_Pro.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitTrack_Pro.Controllers
{
	//[Authorize(Roles = "Admin")]
	public class MembersController(IMemberService memberService) : Controller
	{
		// ════════════════════════════════════════════════════════
		//  GET  /Members  –  paged list with optional search
		// ════════════════════════════════════════════════════════
		[HttpGet]
		public async Task<IActionResult> Index(
			string? search, int page = 1, int pageSize = 10)
		{
			var vm = await memberService.GetPagedMembersAsync(page, pageSize, search);
			return View(vm);
		}

		// ════════════════════════════════════════════════════════
		//  GET  /Members/Details/5
		// ════════════════════════════════════════════════════════
		[HttpGet]
		[Authorize(Roles ="Admin")]
		public async Task<IActionResult> Details(int id)
		{
			var vm = await memberService.GetMemberDetailsAsync(id);
			if (vm is null) return NotFound();
			return View(vm);
		}

		// ════════════════════════════════════════════════════════
		//  GET  /Members/Create
		// ════════════════════════════════════════════════════════
		[HttpGet]
		public async Task<IActionResult> Create()
		{
			var vm = await memberService.GetCreateFormAsync();
			return View(vm);
		}

		// ════════════════════════════════════════════════════════
		//  POST /Members/Create
		// ════════════════════════════════════════════════════════
		[HttpPost, ValidateAntiForgeryToken]
		[Authorize(Roles ="Admin")]
		public async Task<IActionResult> Create(MemberFormViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			var (success, error, newId) = await memberService.CreateMemberAsync(model);
			if (!success)
			{
				ModelState.AddModelError(string.Empty, error!);
				return View(model);
			}

			TempData["Success"] = $"Member \"{model.FullName}\" has been added successfully.";
			return RedirectToAction(nameof(Details), new { id = newId });
		}

		// ════════════════════════════════════════════════════════
		//  GET  /Members/Edit/5
		// ════════════════════════════════════════════════════════
		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var vm = await memberService.GetEditFormAsync(id);
			if (vm is null) return NotFound();
			return View(vm);
		}

		// ════════════════════════════════════════════════════════
		//  POST /Members/Edit
		// ════════════════════════════════════════════════════════
		[HttpPost, ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(MemberFormViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			var (success, error) = await memberService.UpdateMemberAsync(model);
			if (!success)
			{
				ModelState.AddModelError(string.Empty, error!);
				return View(model);
			}
			TempData["Success"] = $"Member \"{model.FullName}\" updated successfully.";
			return RedirectToAction(nameof(Details), new { id = model.Id });
		}

		// ════════════════════════════════════════════════════════
		//  POST /Members/Delete  (AJAX-friendly)
		// ════════════════════════════════════════════════════════
		[HttpPost, ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int id)
		{
			var (success, error) = await memberService.DeleteMemberAsync(id);
			if (!success)
			{
				TempData["Error"] = error;
				return RedirectToAction(nameof(Index));
			}

			TempData["Success"] = "Member deleted successfully.";
			return RedirectToAction(nameof(Index));
		}

		// ════════════════════════════════════════════════════════
		//  GET  /Members/AssignPlan/5
		// ════════════════════════════════════════════════════════
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AssignPlan(int id)
		{
			var vm = await memberService.GetAssignPlanFormAsync(id);
			if (vm is null) return NotFound();
			return View(vm);
		}

		// ════════════════════════════════════════════════════════
		//  POST /Members/AssignPlan
		// ════════════════════════════════════════════════════════
		[HttpPost, ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AssignPlan(AssignPlanViewModel model)
		{
			if (!ModelState.IsValid)
			{
				var vm = await memberService.GetAssignPlanFormAsync(model.MemberId);
				return View(vm);
			}

			var (success, error) = await memberService.AssignPlanAsync(model);
			if (!success)
			{
				ModelState.AddModelError(string.Empty, error!);
				var vm = await memberService.GetAssignPlanFormAsync(model.MemberId);
				return View(vm);
			}

			TempData["Success"] = "Plan assigned successfully.";
			return RedirectToAction(nameof(Details), new { id = model.MemberId });
		}

		[HttpPost, ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteSubscription(int id, int memberId)
		{
			var (success, error) = await memberService.DeleteSubscriptionAsync(id);
			if (!success)
			{
				TempData["Error"] = error;
			}
			else
			{
				TempData["Success"] = "Subscription deleted successfully.";
			}
			return RedirectToAction(nameof(Details), new { id = memberId });
		}

		// ════════════════════════════════════════════════════════
		//  GET  /Members/Dashboard
		// ════════════════════════════════════════════════════════
		[HttpGet]
		[Authorize(Roles = "Member")]
		public async Task<IActionResult> Dashboard()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) return Challenge();

			var vm = await memberService.GetMemberDashboardAsync(userId);
			if (vm is null) return NotFound("Member record not found.");

			return View(vm);
		}
	}
}