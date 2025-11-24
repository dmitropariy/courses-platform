using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace courses_platform.Models
{
    public class Assignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [ForeignKey("Module")]
        public int ModuleId { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Type { get; set; } = null!;          // quiz_single / quiz_multiple / open_text

        [Required, MaxLength(4000)]
        public string QuestionText { get; set; } = null!;

        public Module Module { get; set; } = null!;
        public ICollection<AssignmentOption> Options { get; set; } = new List<AssignmentOption>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
