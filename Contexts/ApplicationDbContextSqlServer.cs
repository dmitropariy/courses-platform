// Contexts/ApplicationDbContextSqlServer.cs
using Microsoft.EntityFrameworkCore;
using courses_platform.Models;

namespace courses_platform.Contexts
{
    public class ApplicationDbContextSqlServer : ApplicationDbContext
    {
        public ApplicationDbContextSqlServer(DbContextOptions<ApplicationDbContextSqlServer> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Force migrations folder
            modelBuilder.HasAnnotation("Relational:MigrationsHistoryTable", "__EFMigrationsHistory");
        }
    }
}