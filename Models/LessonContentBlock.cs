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

        public string BlockType { get; set; } // Наприклад: "text", "image", "video"

        public string Content { get; set; } // text block
        
        public string MediaUrl { get; set; } // image or video URL
        
        public int Order { get; set; }


        public Lesson Lesson { get; set; }
    }
}
