using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using Langbox.Models;

namespace Langbox.Services
{
    public class ChallengeService
    {
        private readonly LangboxDbContext DbContext;

        public ChallengeService(LangboxDbContext context)
        {
            DbContext = context;
        }

        public async Task CreateAsync(Challenge challenge)
        {
            await DbContext.AddAsync(challenge);
            await DbContext.SaveChangesAsync();
        }

        public async Task<Challenge?> GetByIdWithEnvironmentAsync(int id)
        {
            return await DbContext.Challenges
                .Include(c => c.Environment)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Challenge?> GetRandomAsync()
        {
            var challenges = await DbContext.Challenges
                .FromSqlRaw(
                    "SELECT * FROM \"Challenge\" " +
                    "ORDER BY random() " +
                    "LIMIT 1"
                )
                .ToListAsync();

            if (challenges.Count == 0)
            {
                return null;
            }

            return challenges[0];
        }

        public async Task<Challenge?> GetRandomWithoutIdAsync(int id)
        {
            var challenges = await DbContext.Challenges
                .FromSqlRaw(
                    "SELECT * FROM \"Challenge\" " +
                    $"WHERE \"Challenge\".\"Id\" != {id} " +
                    "ORDER BY random() " +
                    "LIMIT 1"
                )
                .ToListAsync();

            if (challenges.Count == 0)
            {
                return null;
            }

            return challenges[0];
        }
    }
}
