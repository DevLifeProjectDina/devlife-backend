// File: Data/ApplicationDbContext.cs
using DevLifeBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DevLifeBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<CodeSnippet> CodeSnippets { get; set; }
    }
}