using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using courses_platform.Models.Dto;
using System.Threading.Tasks;

namespace courses_platform.Models.Dto
{
    public class CreateCourseDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public List<CreateModuleDto> Modules { get; set; } = new();
    }

}
