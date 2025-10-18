using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace courses_platform.Models
{
    public class CourseVerification
    {
        [Key]
        public int VerificationId { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        // Додати в 3 лабі юзера-ревьюера пізніше

        public string Status { get; set; } // approved / rejected / pending

        public string? ReviewComment { get; set; }

        public DateTime? VerifiedAt { get; set; }


        public Course Course { get; set; }
    }
}
