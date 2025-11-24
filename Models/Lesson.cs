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

        [Required, MaxLength(255)]
        public string Title { get; set; } = null!;

        [MaxLength(4000)]
        public string? LessonDescription { get; set; }

        public int OrderNumber { get; set; }

        public Module Module { get; set; } = null!;

        public ICollection<LessonContentBlock> LessonContentBlocks { get; set; } = new List<LessonContentBlock>();
    }
}
