using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace courses_platform.Models
{
    public class LessonContentBlock
    {
        [Key]
        public int LessonContentBlockId { get; set; }

        [ForeignKey("Lesson")]
        public int LessonId { get; set; }

        [Required, MaxLength(50)]
        public string BlockType { get; set; } = null!;     // text, image, video

        [MaxLength(4000)]
        public string? Content { get; set; }              // text block

        [MaxLength(1000)]
        public string? MediaUrl { get; set; }             // image / video URL

        public int Order { get; set; }

        public Lesson Lesson { get; set; } = null!;
    }
}
