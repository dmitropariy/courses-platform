using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace courses_platform.Models
{
    public class ProfessorCourse
    {
        [Key]
        public int Id { get; set; }

        public int ProfessorId { get; set; }

        public int CourseId { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public AppUser? Professor { get; set; }
        public Course? Course { get; set; }
    }
}
