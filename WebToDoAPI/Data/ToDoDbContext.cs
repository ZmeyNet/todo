using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebToDoAPI.Data.Entities;

namespace WebToDoAPI.Data
{
    public class ToDoDbContext : IdentityDbContext<ApplicationUser>
    {
        public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskEntity>()
                .HasOne(p => p.User)
                .WithMany(b => b.Tasks);

        }

        public DbSet<TaskEntity> Tasks { get; set; }
    }
}
