using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SampleApp.API.Models;

namespace SampleApp.API.Data.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<User> Login(string userName, string password)
        {
            var user = await _userManager.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.UserName == userName);
            if (user == null)
            {
                return null;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

            if (!result.Succeeded) return null;

            return user;
        }

        public async Task<User> Register(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return null;
            return user;
        }

        public async Task<bool> UserExists(string userName)
        {
            if (await _userManager.Users.AnyAsync(x => x.UserName == userName))
            {
                return true;
            }
            return false;
        }
    }
}
