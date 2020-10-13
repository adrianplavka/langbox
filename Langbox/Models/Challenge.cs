using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Langbox.Models
{
    [Table("Challenge")]
    public class Challenge
    {
        [Key]
        public int Id { get; set; } = default!;

        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Instructions { get; set; } = "";

        [Required]
        public string MainContent { get; set; } = "";

        [Required]
        public string TestContent { get; set; } = "";

        [ForeignKey("SandboxEnvironment")]
        public string SandboxEnvironmentId { get; set; } = "";
        public SandboxEnvironment Environment { get; set; } = default!;
    }
}
