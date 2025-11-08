using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace courses_platform.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; } = null!;

        [MaxLength(4000)]
        public string? Description { get; set; }

        public int CompletedCount { get; set; }


        public ICollection<Module> Modules { get; set; } = new List<Module>();
        public ICollection<CourseVerification> Verifications { get; set; } = new List<CourseVerification>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public ICollection<StudentCourse>? StudentCourses { get; set; }
        public ICollection<ProfessorCourse>? ProfessorCourses { get; set; }
    }
}
