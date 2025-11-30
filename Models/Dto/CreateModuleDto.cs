using courses_platform.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace courses_platform.Models.Dto
{
    public class CreateModuleDto
    {
        public string Title { get; set; } = null!;
        public string? ModuleDescription { get; set; }
        public int OrderNumber { get; set; }

        public List<CreateLessonDto> Lessons { get; set; } = new();
    }

}
