using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace courses_platform.Models
{
    public class Module
    {
        [Key]
        public int ModuleId { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; } = null!;

        [MaxLength(4000)]
        public string? ModuleDescription { get; set; }

        public int OrderNumber { get; set; }


        public Course Course { get; set; } = null!;
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    } 
}
