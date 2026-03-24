using Microsoft.AspNetCore.Identity;

namespace FitTrack_Pro.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string FullName { get; set; } = string.Empty;
	}
}
