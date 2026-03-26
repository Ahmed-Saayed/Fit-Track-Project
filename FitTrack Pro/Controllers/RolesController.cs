using FitTrack_Pro.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack_Pro.Controllers
{
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        [HttpGet]
        public async Task<IActionResult>Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(AddRoleViewModel request)
        {
           if(ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole
                {
                    Name = request.RoleName
                };
                IdentityResult result = await _roleManager.CreateAsync(identityRole);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Roles");
                }
                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(request);
        }
        public IActionResult Index()
        {
            IEnumerable<AddRoleViewModel> roles = _roleManager.Roles.Select(r => new AddRoleViewModel
            {
                RoleName = r.Name
            });
            return View(roles);
        }
    }
}
