using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Langbox.Models;

namespace Langbox.Services
{
    public class ChallengeService
    {
        private readonly LangboxDbContext _dbContext;

        public ChallengeService(LangboxDbContext context)
        {
            _dbContext = context;
        }

        public async Task CreateAsync(Challenge challenge)
        {
            await _dbContext.AddAsync(challenge);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Challenge?> GetByIdWithEnvironmentAsync(int id)
        {
            return await _dbContext.Challenges
                .Include(c => c.Environment)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Challenge?> GetRandomAsync()
        {
            var challenges = await _dbContext.Challenges
                .FromSqlRaw(
                    "SELECT * FROM \"Challenge\" " +
                    "ORDER BY random() " +
                    "LIMIT 1"
                )
                .ToListAsync();

            return challenges.Count == 0 ? null : challenges[0];
        }

        public async Task<Challenge?> GetRandomWithoutIdWithEnvironmentAsync(int id)
        {
            var challenges = await _dbContext.Challenges
                .FromSqlRaw(
                    "SELECT * FROM \"Challenge\" " +
                    $"WHERE \"Challenge\".\"Id\" != {id} " +
                    "ORDER BY random() " +
                    "LIMIT 1"
                )
                .Include(c => c.Environment)
                .ToListAsync();

            return challenges.Count == 0 ? null : challenges[0];
        }
    }
}
