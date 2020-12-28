using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleApp.API.Data.Interfaces;
using SampleApp.API.Dtos;
using SampleApp.API.Extensions;
using SampleApp.API.Helpers;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogUserActivity))]
    public class UsersController : ControllerBase
    {
        private readonly ISampleAppService _sampleAppService;
        private readonly IMapper _mapper;
        public UsersController(ISampleAppService sampleAppService, IMapper mapper)
        {
            _sampleAppService = sampleAppService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            var user = await _sampleAppService.GetUserByUserName(User.Identity.Name);
            userParams.CurrentUsername = user.UserName;
            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = user.Gender == "male" ? "female" : "male";
            }
            var users = await _sampleAppService.GetUsers(userParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);
            return Ok(usersToReturn);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _sampleAppService.GetUser(id);
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var userFromRepo = await _sampleAppService.GetUser(id);
            _mapper.Map(userForUpdateDto, userFromRepo);
            if (await _sampleAppService.SaveAll())
            {
                return NoContent();
            }
            throw new Exception($"Updating user {id} failed on save");

        }

    }
}
