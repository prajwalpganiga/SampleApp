using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using SampleApp.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            if (!userManager.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                var roles = new List<Role>
                {
                    new Role{ Name = "Member"},
                    new Role{ Name = "Admin"},
                    new Role{ Name = "Moderator" }
                };

                foreach(var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }

                foreach (var user in users)
                {
                    user.UserName = user.UserName.ToLower();
                    await userManager.CreateAsync(user, "Pass123!");
                    await userManager.AddToRoleAsync(user, "Member");
                }

                var admin = new User
                {
                    UserName = "admin"
                };

                await userManager.CreateAsync(admin, "Pass123!");
                await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
            }
        }
    }
}