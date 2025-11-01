using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace courses_platform.Models
{
    public class StudentCourse
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int CourseId { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        public bool IsCompleted { get; set; }

        public DateTime? CompletedTime { get; set;}

        public AppUser? Student { get; set; }
        public Course? Course { get; set; }
    }
}
