using FitTrack_Pro.Models;
using Microsoft.AspNetCore.Identity;

namespace FitTrack_Pro.Data
{
	public static class ContextSeed
	{
		public static async Task SeedRolesAndAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			
			string[] roles = { "Admin", "Receptionist", "Trainer", "Member" };
			foreach (var role in roles)
			{
				if (!await roleManager.RoleExistsAsync(role))
					await roleManager.CreateAsync(new IdentityRole(role));
			}

			var adminUser = new ApplicationUser
			{
				UserName = "admin",
				Email = "admin@fittrack.com",
				FullName = "System Admin",
				EmailConfirmed = true
			};

			if (userManager.Users.All(u => u.UserName != adminUser.UserName))
			{
				await userManager.CreateAsync(adminUser, "1234"); 
				await userManager.AddToRoleAsync(adminUser, "Admin");
			}
		}
	}
}
