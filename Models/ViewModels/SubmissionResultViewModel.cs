namespace courses_platform.Models.ViewModels
{
    public class SubmissionResultViewModel
    {
        public int SubmissionId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public string ModuleTitle { get; set; } = null!;
        public string AssignmentTitle { get; set; } = null!;
        public string AnswerText { get; set; } = null!;
        public bool? IsCorrect { get; set; }
        public DateTime? GradedAt { get; set; }
    }
}
