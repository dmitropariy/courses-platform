namespace courses_platform.Models
{
    public class UserProfileViewModel
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public string? MediaUrl { get; set; }
        public string? SocialLinks { get; set; }
        public IFormFile? PhotoFile { get; set; }
    }
}
