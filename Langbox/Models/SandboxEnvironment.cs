using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Langbox.Models
{
    [Table("SandboxEnvironment")]
    public class SandboxEnvironment
    {
        [Key]
        public string TemplateName { get; set; } = "";

        [Required]
        public string Language { get; set; } = "";

        [Required]
        public string MainBoilerplate { get; set; } = "";

        [Required]
        public string TestBoilerplate { get; set; } = "";

        [Required]
        public string MainFileName { get; set; } = "";

        [Required]
        public string TestFileName { get; set; } = "";

        [Required]
        public string DockerMainPath { get; set; } = "";

        [Required]
        public string DockerTestPath { get; set; } = "";

        [Required]
        public string DockerImage { get; set; } = "";

        [Required]
        public string DockerCommand { get; set; } = "";

        public ICollection<Challenge> Challenges { get; set; } = default!;
    }
}
