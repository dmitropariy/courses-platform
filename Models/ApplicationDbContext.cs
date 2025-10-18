using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace courses_platform.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonContentBlock> LessonContentBlocks { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentOption> AssignmentOptions { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<CourseVerification> CourseVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === Визначення зв’язків ===

            // Course → Module (1→багато)
            modelBuilder.Entity<Module>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Module → Lesson (1→багато)
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lesson → LessonContentBlock (1→багато)
            modelBuilder.Entity<LessonContentBlock>()
                .HasOne(cb => cb.Lesson)
                .WithMany(l => l.LessonContentBlocks)
                .HasForeignKey(cb => cb.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Module → Assignment (1→багато)
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Module)
                .WithMany(m => m.Assignments)
                .HasForeignKey(a => a.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Assignment → AssignmentOption (1→багато)
            modelBuilder.Entity<AssignmentOption>()
                .HasOne(o => o.Assignment)
                .WithMany(a => a.Options)
                .HasForeignKey(o => o.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Assignment → Submission (1→багато)
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Course → Certificate (1→багато)
            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Course)
                .WithMany(cou => cou.Certificates)
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Course → CourseVerification (1→багато)
            modelBuilder.Entity<CourseVerification>()
                .HasOne(v => v.Course)
                .WithMany(c => c.Verifications)
                .HasForeignKey(v => v.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
