using FitTrack_Pro.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FitTrack_Pro.Helpers
{
    public class AccountHelper: IAccountHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AccountHelper(UserManager<ApplicationUser>userManager , SignInManager<ApplicationUser> signInManager)
        {
             _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<string?> RegisterUser(RegisterModel registerViewModel)
        {
            var user = new ApplicationUser
            {
                UserName = registerViewModel.UserName,
                PasswordHash = registerViewModel.Password,
                FullName = registerViewModel.FullName

            };
            IdentityResult result = _userManager.CreateAsync(user, registerViewModel.Password).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, registerViewModel.Role);
                await _signInManager.SignInAsync(user, false);
                return user.Id;
            } else return null;
        }
    }
}
