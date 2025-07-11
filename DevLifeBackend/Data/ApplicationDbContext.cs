
using DevLifeBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DevLifeBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.ExperienceLevel)
                .HasConversion<string>(); 
        }
    }
}