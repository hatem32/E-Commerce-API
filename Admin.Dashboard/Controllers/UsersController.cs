using AdminDashboard.Models.Roles;
using AdminDashboard.Models.Users;
using E_Commerce.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                Email = u.Email,
                UserName = u.UserName,
                Roles = _userManager.GetRolesAsync(u).Result,
            }).ToListAsync();

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            var roles = await _roleManager.Roles.ToListAsync();

            var userModel = new UserRoleViewModel()
            {

                UserId = user.Id,
                UserName = user.UserName,

                Roles = roles.Select(r => new UpdatedRoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    IsSelected = _userManager.IsInRoleAsync(user, r.Name).Result
                }).ToList()
            };

            return View(userModel);

        }


        [HttpPost]
        public async Task<IActionResult> Edit(UserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            var roleForUser = await _userManager.GetRolesAsync(user);

            //Role was Granted => Uncheck For Role (Remove Role)
            //Role was not Granted => Check For Role (Add Role)

            foreach (var role in model.Roles)
            {
                if (roleForUser.Any(r => r == role.Name) && !role.IsSelected)
                    await _userManager.RemoveFromRoleAsync(user, role.Name);

                if (!roleForUser.Any(r => r == role.Name) && role.IsSelected)
                    await _userManager.AddToRoleAsync(user, role.Name);
            }

            return RedirectToAction(nameof(Index));

        }

    }
}
