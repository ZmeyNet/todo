using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using WebToDoAPI.Data.Entities;

namespace WebToDoAPI.Data
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<TaskEntity> Tasks { get; set; }
    }
}
