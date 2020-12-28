using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SampleApp.API.Data;
using SampleApp.API.Dtos;
using SampleApp.API.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        public AuthController(IAuthService authService, IConfiguration config, IMapper mapper, UserManager<User> userManager)
        {
            _authService = authService;
            _config = config;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //  validate request
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            if (await _authService.UserExists(userForRegisterDto.Username))
            {
                return BadRequest("Username Already Exists");

            }
            var user = _mapper.Map<User>(userForRegisterDto);

            user.UserName = userForRegisterDto.Username.ToLower();

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            await _authService.Register(user, userForRegisterDto.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _authService.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            if (userFromRepo == null)
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name,userFromRepo.UserName)
            };

            var roles = await _userManager.GetRolesAsync(userFromRepo);

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var user = _mapper.Map<UserForListDto>(userFromRepo);
            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            });
        }

    }
}
