// Contexts/ApplicationDbContextPostgreSQL.cs
using courses_platform.Contexts;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContextPostgreSQL : ApplicationDbContext
{
    public ApplicationDbContextPostgreSQL(DbContextOptions<ApplicationDbContextPostgreSQL> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Force migrations folder
        modelBuilder.HasAnnotation("Relational:MigrationsHistoryTable", "__EFMigrationsHistory");
    }
}