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
        Task<bool> SaveAll();
        Task<IEnumerable<User>> GetUsers();
        Task<User> GetUser(int id);
    }
}