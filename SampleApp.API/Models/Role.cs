using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SampleApp.API.Models
{
    public class Role : IdentityRole<int>
    {
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
