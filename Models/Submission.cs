using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace courses_platform.Models
{
    public class Submission
    {
        [Key]
        public int SubmissionId { get; set; }

        [ForeignKey("Assignment")]
        public int AssignmentId { get; set; }

        // Додати в 3 лабі зв'язок з юзером, який зробив сабмішн

        public string AnswerText { get; set; } // для опен текст відповідей

        public string SelectedOptions { get; set; } // JSON string, для мульти чойс відповідей

        public bool? IsCorrect { get; set; }

        public DateTime? GradedAt { get; set; }


        public Assignment Assignment { get; set; }
    }
}
