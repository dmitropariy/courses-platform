using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace courses_platform.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; }

        // Додати в 3 лабі з'єднання з Юзером-Автором

        public string Description { get; set; }

        public int CompletedCount { get; set; }


        public ICollection<Module> Modules { get; set; }
        public ICollection<CourseVerification> Verifications { get; set; } // до багатьох тому що може бути внесена правка і перевірена знову
        public ICollection<Certificate> Certificates { get; set; }
    }
}
