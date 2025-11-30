namespace courses_platform.Models.Dto
{
    public class CourseDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int CompletedCount { get; set; }
    }
}
