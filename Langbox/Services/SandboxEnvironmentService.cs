using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Langbox.Models;

namespace Langbox.Services
{
    public class SandboxEnvironmentService
    {
        private readonly LangboxDbContext DbContext;

        public SandboxEnvironmentService(LangboxDbContext context)
        {
            DbContext = context;
        }

        public ICollection<SandboxEnvironment> ListAll()
        {
            return DbContext.SandboxEnviornments.ToList();
        }

        public async Task<SandboxEnvironment?> GetByTemplateNameAsync(string templateName)
        {
            return await DbContext.SandboxEnviornments.FindAsync(templateName);
        }
    }
}
