using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.API.Data.Interfaces
{
    public interface IUnitOfWork
    {
        ISampleAppService userService { get; }
        IMessageService messageService { get; }
        ILikesService likesService { get; }
        Task<bool> Complete();
        bool HasChanges();
    }
}
