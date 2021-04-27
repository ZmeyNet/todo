using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebToDoAPI.Data
{
    public class ToDoDbContext : IdentityDbContext<ApplicationUser>
    {
        public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
        {
        }
    }
}
