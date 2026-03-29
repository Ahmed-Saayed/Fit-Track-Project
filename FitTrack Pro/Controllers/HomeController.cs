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
			var viewModel = await _service.GetDashboardDataAsync();
			return View(viewModel);
        }
	}
}
