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

        // Додати в 3 лабі зв'язок з юзером, який отримав сертифікат

        public DateTime IssueDate { get; set; }

        public string CertificateUrl { get; set; }


        public Course Course { get; set; }
    }
}
