using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Langbox.Models;

namespace Langbox.Services
{
    public class SandboxEnvironmentService
    {
        private readonly LangboxDbContext _dbContext;

        public SandboxEnvironmentService(LangboxDbContext context)
        {
            _dbContext = context;
        }

        public ICollection<SandboxEnvironment> ListAll()
        {
            return _dbContext.SandboxEnviornments.ToList();
        }

        public async Task<SandboxEnvironment?> GetByTemplateNameAsync(string templateName)
        {
            return await _dbContext.SandboxEnviornments.FindAsync(templateName);
        }
    }
}
