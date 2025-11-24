namespace courses_platform.Models.ViewModels
{
    public class StudentProfileViewModel
    {
        public UserProfileViewModel Profile { get; set; } = null!;
        public List<StudentCourseViewModel> Courses { get; set; } = new();
        public bool CanEdit { get; set; }
    }
}
