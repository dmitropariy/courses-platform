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

        [Required, MaxLength(1000)]
        public string Text { get; set; } = null!;

        public bool IsCorrect { get; set; }

        public Assignment Assignment { get; set; } = null!;
    }
}
