// Contexts/ApplicationDbContextSqlite.cs
using courses_platform.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContextSqlite : ApplicationDbContext
{
    public ApplicationDbContextSqlite(DbContextOptions<ApplicationDbContextSqlite> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Force migrations folder
        modelBuilder.HasAnnotation("Relational:MigrationsHistoryTable", "__EFMigrationsHistory");
    }
}