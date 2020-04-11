using SampleApp.API.Models;
using System.Threading.Tasks;

namespace SampleApp.API.Data
{
    public interface IAuthService
    {
        Task<User> Register(User user, string password);
        Task<User> Login(string userName, string password);
        Task<bool> UserExists(string userName);
    }
}