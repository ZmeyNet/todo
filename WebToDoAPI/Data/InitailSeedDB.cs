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
        private const string userRoleName = "user";
        private const string adminRoleName = "admin";

        public static void Initialize(IServiceProvider serviceProvider)
        {
            const string pwdDefault = "Pwd@@123";

            var context = serviceProvider.GetRequiredService<ToDoDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            //recrete DB each time 
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            if (!context.Users.Any())
            {
                var user = new ApplicationUser()
                {
                    Email = "user@example.com",
                    UserName = "user",
                    EmailConfirmed = true,                    
                    SecurityStamp = Guid.NewGuid().ToString("N")
                };
                var lockedUser = new ApplicationUser()
                {
                    Email = "locked@example.com",
                    UserName = "lockedUser",
                    LockoutEnabled = true,
                    SecurityStamp = Guid.NewGuid().ToString("N")
                };
                var userWithAdminRights = new ApplicationUser()
                {
                    Email = "admin@example.com",
                    UserName = "adimn",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("N")
                };
                
               userManager.CreateAsync(user, pwdDefault);
               userManager.CreateAsync(lockedUser, pwdDefault);
               userManager.CreateAsync(userWithAdminRights, pwdDefault);

               userManager.AddToRolesAsync(user, new[] { userRoleName });
               userManager.AddToRolesAsync(lockedUser, new[] { userRoleName });
               userManager.AddToRolesAsync(userWithAdminRights, new[] { adminRoleName, userRoleName });


            }
        }

    }
}
