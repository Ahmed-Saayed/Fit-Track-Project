using FitTrack_Pro.Interfaces;
using FitTrack_Pro.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack_Pro.Controllers
{
    public class GymClassesController(IGymClassService gymClassService) : Controller
    {
        // ════════════════════════════════════════════════════════
        //  GET  /GymClasses  –  Weekly Schedule or Paged List
        // ════════════════════════════════════════════════════════
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(
            string? search, int page = 1, int pageSize = 10, string view = "schedule", DateTime? startDate = null)
        {
            if (view == "list")
            {
                var vm = await gymClassService.GetPagedGymClassesAsync(page, pageSize, search);
                ViewData["ViewMode"] = "list";
                return View(vm);
            }

            var scheduleVm = await gymClassService.GetWeeklyScheduleAsync(startDate);
            ViewData["ViewMode"] = "schedule";
            return View("Schedule", scheduleVm);
        }

        // ════════════════════════════════════════════════════════
        //  GET  /GymClasses/Details/5
        // ════════════════════════════════════════════════════════
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var vm = await gymClassService.GetGymClassDetailsAsync(id);
            if (vm is null) return NotFound();
            return View(vm);
        }

        // ════════════════════════════════════════════════════════
        //  GET  /GymClasses/Create
        // ════════════════════════════════════════════════════════
        [HttpGet]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Create()
        {
            var vm = await gymClassService.GetCreateFormAsync();
            return View(vm);
        }

        // ════════════════════════════════════════════════════════
        //  POST /GymClasses/Create
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(GymClassFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TrainerOptions = (await gymClassService.GetCreateFormAsync()).TrainerOptions;
                return View(model);
            }

            var (success, error, newId) = await gymClassService.CreateGymClassAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                model.TrainerOptions = (await gymClassService.GetCreateFormAsync()).TrainerOptions;
                return View(model);
            }

            TempData["Success"] = $"Class \"{model.Name}\" has been created successfully.";
            return RedirectToAction(nameof(Details), new { id = newId });
        }

        // ════════════════════════════════════════════════════════
        //  GET  /GymClasses/Edit/5
        // ════════════════════════════════════════════════════════
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await gymClassService.GetEditFormAsync(id);
            if (vm is null) return NotFound();
            return View(vm);
        }

        // ════════════════════════════════════════════════════════
        //  POST /GymClasses/Edit
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(GymClassFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TrainerOptions = (await gymClassService.GetEditFormAsync(model.Id))?.TrainerOptions
                    ?? (await gymClassService.GetCreateFormAsync()).TrainerOptions;
                return View(model);
            }

            var (success, error) = await gymClassService.UpdateGymClassAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                model.TrainerOptions = (await gymClassService.GetEditFormAsync(model.Id))?.TrainerOptions
                    ?? (await gymClassService.GetCreateFormAsync()).TrainerOptions;
                return View(model);
            }

            TempData["Success"] = $"Class \"{model.Name}\" updated successfully.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        // ════════════════════════════════════════════════════════
        //  POST /GymClasses/Delete
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await gymClassService.DeleteGymClassAsync(id);
            if (!success)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Gym class deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
