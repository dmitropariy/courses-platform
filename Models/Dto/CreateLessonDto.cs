using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace courses_platform.Models.Dto
{
    public class CreateLessonDto
    {
        public string Title { get; set; } = null!;
        public string? LessonDescription { get; set; }
        public int OrderNumber { get; set; }
    }
}
