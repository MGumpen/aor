using AOR.Data;
using AOR.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace AOR.Repositories
{
    public class ObstacleRepository : IObstacleRepository
    {
        private readonly AorDbContext _context;

        public ObstacleRepository(AorDbContext context)
        {
            _context = context;
        }

        public async Task<List<ObstacleData>> GetAllAsync()
        {
            return await _context.Obstacles
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<ObstacleData?> GetByIdAsync(int obstacleId)
        {
            return await _context.Obstacles
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.ObstacleId == obstacleId);
        }

        public async Task<bool> ExistsAsync(int obstacleId)
        {
            return await _context.Obstacles
                .AnyAsync(o => o.ObstacleId == obstacleId);
        }

        public async Task AddAsync(ObstacleData obstacle)
        {
            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ObstacleData obstacle)
        {
            _context.Obstacles.Update(obstacle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int obstacleId)
        {
            var obstacle = await _context.Obstacles
                .FirstOrDefaultAsync(o => o.ObstacleId == obstacleId);

            if (obstacle == null)
            {
                return;
            }

            _context.Obstacles.Remove(obstacle);
            await _context.SaveChangesAsync();
        }
    }
}