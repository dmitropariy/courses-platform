namespace courses_platform.Models.ViewModels
{
    public class SubmissionSearchModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<int>? SelectedCourseIds { get; set; }
        public string? AssignmentType { get; set; }
        public string? AnswerPrefix { get; set; }
        public string? AnswerSuffix { get; set; }
    }
}
