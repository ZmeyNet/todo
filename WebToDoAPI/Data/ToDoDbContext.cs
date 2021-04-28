
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebToDoAPI.Data.Entities;

namespace WebToDoAPI.Data
{
    public class ToDoDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            //bypassing issues with MySQL composite PK creation 
            //dirty hack todo remove me 
            modelBuilder.Entity<ApplicationUser>().Property(u => u.Id).HasMaxLength(127);
            modelBuilder.Entity<ApplicationRole>().Property(u => u.Id).HasMaxLength(127);

            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.Property(m => m.LoginProvider).HasMaxLength(127);
                entity.Property(m => m.ProviderKey).HasMaxLength(127);
            });

            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.Property(m => m.UserId).HasMaxLength(127);
                entity.Property(m => m.RoleId).HasMaxLength(127);
            });

            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.Property(m => m.UserId).HasMaxLength(127);
                entity.Property(m => m.LoginProvider).HasMaxLength(127);
                entity.Property(m => m.Name).HasMaxLength(127);
            });

            modelBuilder.Entity<TaskEntity>()
                .HasOne(p => p.User)
                .WithMany(b => b.Tasks);

        }

        public DbSet<TaskEntity> Tasks { get; set; }
    }
}
