using FitTrack_Pro.Interfaces;
using FitTrack_Pro.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack_Pro.Controllers
{
    public class MembersController(IMemberService memberService) : Controller
    {
        // ════════════════════════════════════════════════════════
        //  GET  /Members  –  paged list with optional search
        // ════════════════════════════════════════════════════════
        [HttpGet]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var vm = await memberService.GetCreateFormAsync();
            return View(vm);
        }

        // ════════════════════════════════════════════════════════
        //  POST /Members/Create
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
    }
}