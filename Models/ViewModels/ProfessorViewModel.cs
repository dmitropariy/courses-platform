namespace courses_platform.Models.ViewModels
{
    public class ProfessorViewModel
    {
        public int ProfessorId { get; set; }
        public string ExternalUserId { get; set; } = null!;
        public int CourseCount { get; set; }
    }
}
