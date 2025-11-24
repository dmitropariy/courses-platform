namespace courses_platform.Models.ViewModels
{
    public class ProfessorCourseViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CompletedCount { get; set; }
    }
}
