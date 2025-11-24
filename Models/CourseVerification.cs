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

        [Required, MaxLength(50)]
        public string Status { get; set; } = null!;        // approved / rejected / pending

        [MaxLength(2000)]
        public string? ReviewComment { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public Course Course { get; set; } = null!;
    }
}
