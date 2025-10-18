using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace courses_platform.Models
{
    public class AssignmentOption
    {
        [Key]
        public int OptionId { get; set; }

        [ForeignKey("Assignment")]
        public int AssignmentId { get; set; }

        public string Text { get; set; }

        public bool IsCorrect { get; set; }


        public Assignment Assignment { get; set; }
    }
}
