using Microsoft.AspNetCore.Mvc.Filters;
using SampleApp.API.Data.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace SampleApp.API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();
            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

            var userId = int.Parse(resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var repo = resultContext.HttpContext.RequestServices.GetService<ISampleAppService>();
            var user = await repo.GetUserById(userId);
            if (user != null)
            {
                user.LastActive = DateTime.Now;
            }
            await repo.SaveAll();
        }
    }
}
