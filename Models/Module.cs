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

        public string Title { get; set; }

        public string ModuleDescription { get; set; }

        public int OrderNumber { get; set; }


        public Course Course { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
        public ICollection<Assignment> Assignments { get; set; }
    }
}
