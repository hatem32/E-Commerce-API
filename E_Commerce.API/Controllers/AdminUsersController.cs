using E_Commerce.API.DTOs;
using E_Commerce.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.API.Controllers
{
    // Direct Identity access (UserManager/RoleManager), same pattern the old MVC
    // Admin Dashboard used - there's no separate "user management" domain service.
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ApiBaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: api/adminusers
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetUsers(CancellationToken ct)
        {
            var users = await _userManager.Users.ToListAsync(ct);
            var result = new List<AdminUserDto>(users.Count);

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new AdminUserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    DisplayName = user.DisplayName,
                    Roles = roles
                });
            }

            return Ok(result);
        }

        // PUT: api/adminusers/{id}/roles
        [HttpPut("{id}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserRoles(string id, UpdateUserRolesDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToRemove = currentRoles.Except(dto.Roles).ToList();
            var rolesToAdd = dto.Roles.Except(currentRoles).ToList();

            if (rolesToRemove.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            if (rolesToAdd.Count > 0)
                await _userManager.AddToRolesAsync(user, rolesToAdd);

            return Ok();
        }

        // GET: api/adminusers/roles
        [HttpGet("roles")]
        public async Task<ActionResult<IReadOnlyList<AdminRoleDto>>> GetRoles(CancellationToken ct)
        {
            var roles = await _roleManager.Roles
                .Select(r => new AdminRoleDto { Id = r.Id, Name = r.Name ?? string.Empty })
                .ToListAsync(ct);

            return Ok(roles);
        }

        // POST: api/adminusers/roles
        [HttpPost("roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRole(CreateRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Role name is required.");

            if (await _roleManager.RoleExistsAsync(dto.Name))
                return BadRequest($"A role named '{dto.Name}' already exists.");

            var result = await _roleManager.CreateAsync(new IdentityRole(dto.Name));

            if (!result.Succeeded)
                return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));

            return Ok();
        }

        // DELETE: api/adminusers/roles/{id}
        [HttpDelete("roles/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
                return NotFound();

            await _roleManager.DeleteAsync(role);
            return Ok();
        }
    }
}