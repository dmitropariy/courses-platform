namespace courses_platform.Models
{
    public class ProfessorProfileViewModel
    {
        public UserProfileViewModel Profile { get; set; } = null!;
        public List<ProfessorCourseViewModel> Courses { get; set; } = new();
        public bool CanEdit { get; set; }
    }
}
