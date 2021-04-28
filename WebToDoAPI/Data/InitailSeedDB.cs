using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using WebToDoAPI.Configuration;
using WebToDoAPI.Data.Entities;

namespace WebToDoAPI.Data
{
    public class InitailSeedDB
    {
        private const string defaultUserName = "user";

        public static void Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ToDoDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            //re crete DB each time 
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if (!context.Users.Any())
            {
                FillInSampleUsers(userManager);
                FillInSampleNotes(context, userManager);
            }
        }

        private static void FillInSampleNotes(ToDoDbContext context, UserManager<ApplicationUser> userManager)
        {
            //get default user
            var userFromUM = userManager.Users.FirstOrDefault(c => c.EmailConfirmed && c.UserName == defaultUserName);
            Debug.Assert(userFromUM != null);
            var user = context.Users.Find(userFromUM.Id);
                
            //fill in with some test data
            context.Tasks.Add(new TaskEntity{Name = "test task 1",Description = "some description for task 1",User = user});
            context.Tasks.Add(new TaskEntity{Name = "test task 2",Description = "some description for task 2",User = user});
            context.Tasks.Add(new TaskEntity{Name = "test task 3",Description = "some description for task 3",User = user});
            context.Tasks.Add(new TaskEntity{Name = "test task 4",IsCompleted = true, Description = "some description for task 4",User = user});
            
            context.SaveChanges();

        }

        private static void FillInSampleUsers(UserManager<ApplicationUser> userManager)
        {
            const string pwdDefault = "Pwd@@123";

            var user = new ApplicationUser
            {
                Email = "user@example.com",
                UserName = defaultUserName,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("N")
            };
            var lockedUser = new ApplicationUser
            {
                Email = "locked@example.com",
                UserName = "lockedUser",
                LockoutEnabled = true,
                SecurityStamp = Guid.NewGuid().ToString("N")
            };
            var userWithAdminRights = new ApplicationUser
            {
                Email = "admin@example.com",
                UserName = "admin",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("N")
            };

            _ = userManager.CreateAsync(user, pwdDefault).Result;
            _ = userManager.CreateAsync(userWithAdminRights, pwdDefault);
            _ = userManager.AddToRolesAsync(user, new[] { AppUserRoles.User });
            _ = userManager.AddToRolesAsync(lockedUser, new[] { AppUserRoles.User });
            _ = userManager.AddToRolesAsync(userWithAdminRights, new[] { AppUserRoles.Administrator, AppUserRoles.User });
        }
    }
}
