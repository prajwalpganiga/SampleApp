using SampleApp.API.Helpers;
using SampleApp.API.Models;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApp.API.Data.Interfaces
{
    public interface ISampleAppService
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<PagedList<User>> GetUsers(UserParams userParams);
        Task<User> GetUserById(int id);
        Task<User> GetUserByUserName(string username);
        Task<User> GetUser(int id);
        Task<Photo> GetPhoto(int id);
        Task<Photo> GetMainPhotoForUser(int userId);
        Task<string> GetUserGender(string username);
    }
}