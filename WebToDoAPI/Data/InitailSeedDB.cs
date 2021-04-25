using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebToDoAPI.Data
{
    public class InitailSeedDB
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ToDoDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            context.Database.EnsureCreated();
            
            if (!context.Users.Any())
            {
                ApplicationUser user = new ApplicationUser()
                {                    
                    Email = "user@example.com",
                    UserName = "user",
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                ApplicationUser lockedUser = new ApplicationUser()
                {
                    Email = "user1@example.com",
                    UserName = "user1",
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                ApplicationUser userWithAdminRights = new ApplicationUser()
                {
                    Email = "admin@example.com",
                    UserName = "adimn",
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                userManager.CreateAsync(user, "pwd@@123");
                userManager.CreateAsync(lockedUser, "pwd@@123");
                userManager.CreateAsync(userWithAdminRights, "pwd@@123");
            }
        }

    }
}
