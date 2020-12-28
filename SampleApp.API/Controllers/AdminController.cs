using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        public AdminController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("user-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                .Include(r => r.UserRoles).ThenInclude(r => r.Role)
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                }).ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Only Admin or Moderators can see this");
        }
    }
}
