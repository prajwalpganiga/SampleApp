using SampleApp.API.Dtos;
using SampleApp.API.Helpers;
using SampleApp.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApp.API.Data.Interfaces
{
    public interface ILikesService
    {
        Task<UserLike> GetUserLike(int sourceUserId, int likedUserId);
        Task<User> GetUserWithLikes(int userId);
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
    }
}
