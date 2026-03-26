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
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
                var user = await _userManager.FindByNameAsync(model.Username);
				if (user != null)
				{
					var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);

					if (result.Succeeded)
					{
                        List<Claim> claims = new List<Claim>();
                        claims.Add(new Claim("FullName", user.FullName));
                        await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, claims);
                        if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Receptionist"))
							return RedirectToAction("Index", "Home");
						else if (await _userManager.IsInRoleAsync(user, "Trainer"))
							return RedirectToAction("Index", "Home");
						else
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
	}
}