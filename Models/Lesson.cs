using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace courses_platform.Models
{
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }

        [ForeignKey("Module")]
        public int ModuleId { get; set; }

        public string Title { get; set; }

        public string LessonDescription { get; set; }

        public int OrderNumber { get; set; }


        public Module Module { get; set; }

        public ICollection<LessonContentBlock> LessonContentBlocks { get; set; } = new List<LessonContentBlock>();
    }
}
