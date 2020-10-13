using Microsoft.EntityFrameworkCore;

namespace Langbox.Models
{
    public class LangboxDbContext : DbContext
    {
        public LangboxDbContext(DbContextOptions<LangboxDbContext> options) : base(options) { }

        public DbSet<SandboxEnvironment> SandboxEnviornments { get; set; } = default!;

        public DbSet<Challenge> Challenges { get; set; } = default!;
    }
}
