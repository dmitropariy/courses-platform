// Contexts/ApplicationDbContextInMemory.cs
using courses_platform.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContextInMemory : ApplicationDbContext
{
    public ApplicationDbContextInMemory(DbContextOptions<ApplicationDbContextInMemory> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Force migrations folder
        modelBuilder.HasAnnotation("Relational:MigrationsHistoryTable", "__EFMigrationsHistory");
    }
}