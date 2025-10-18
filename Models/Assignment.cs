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

        public string Title { get; set; }

        public string Type { get; set; } // quiz_single / quiz_multiple / open_text

        public string QuestionText { get; set; }


        public Module Module { get; set; }
        public ICollection<AssignmentOption> Options { get; set; }
        public ICollection<Submission> Submissions { get; set; }
    }
}
