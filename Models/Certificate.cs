using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace courses_platform.Models
{
    public class Certificate
    {
        [Key]
        public int CertificateId { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        public DateTime IssueDate { get; set; }

        [Required, MaxLength(1000)]
        public string CertificateUrl { get; set; } = null!;


        public Course Course { get; set; } = null!;
    }
}
