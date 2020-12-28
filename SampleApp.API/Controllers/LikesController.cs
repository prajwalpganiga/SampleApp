using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleApp.API.Data.Interfaces;
using SampleApp.API.Dtos;
using SampleApp.API.Extensions;
using SampleApp.API.Helpers;
using SampleApp.API.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class LikesController : ControllerBase
    {
        private readonly ISampleAppService _userService;
        private readonly ILikesService _likesService;
        public LikesController(ISampleAppService userService, ILikesService likesService)
        {
            _userService = userService;
            _likesService = likesService;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var likedUser = await _userService.GetUserByUserName(username);
            var sourceUser = await _likesService.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

            var userLike = await _likesService.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("You already like this user");

            userLike = new UserLike
            {
                SourceUserId = sourceUser.Id,
                LikedUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await _userService.SaveAll()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var users = await _likesService.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }
    }
}
