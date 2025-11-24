using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace courses_platform.Models
{
    public class AppUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string ExternalUserId { get; set; } = string.Empty; // ID з AuthServer (sub)

        public ICollection<StudentCourse>? StudentCourses { get; set; }
        public ICollection<ProfessorCourse>? ProfessorCourses { get; set; }
    }
}
