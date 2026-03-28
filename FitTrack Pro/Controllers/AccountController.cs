using FitTrack_Pro.Models;
using FitTrack_Pro.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitTrack_Pro.Controllers
{
	public class AccountController : Controller
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;

		public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
		{
			_signInManager = signInManager;
			_userManager = userManager;
		}

		[HttpGet]
		public IActionResult Login() => View();

		[HttpPost]
		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByNameAsync(model.Username);
				if (user != null)
				{
					var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

					if (result.Succeeded)
					{
						var claims = new List<Claim> { new Claim("FullName", user.FullName) };
						await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, claims);

						var userRoles = await _userManager.GetRolesAsync(user);

						if (userRoles.Contains("Admin") || userRoles.Contains("Receptionist"))
						{
							return RedirectToAction("Index", "Home"); 
						}
						else if (userRoles.Contains("Trainer"))
						{
							return RedirectToAction("Profile", "Trainers"); 
						}
						else if (userRoles.Contains("Member"))
						{
							return RedirectToAction("Dashboard", "Members"); 
						}

						return RedirectToAction("Index", "Home");
					}
				}
				ModelState.AddModelError("", "Invalid Username or Password");
			}
			return View(model);
		}

		//[HttpPost]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

		[HttpGet]
		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}