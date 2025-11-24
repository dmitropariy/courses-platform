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

        [MaxLength(4000)]
        public string? AnswerText { get; set; }           // open-text answer

        [MaxLength(1000)]
        public string? SelectedOptions { get; set; }      // comma-separated IDs

        public bool? IsCorrect { get; set; }
        public DateTime? GradedAt { get; set; }

        public Assignment Assignment { get; set; } = null!;
    }
}
