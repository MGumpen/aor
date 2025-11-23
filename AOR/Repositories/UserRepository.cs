using AOR.Data;
using Microsoft.EntityFrameworkCore;

namespace AOR.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AorDbContext _context;

        public UserRepository(AorDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllWithOrganizationAsync()
        {
            return await _context.Users
                .Include(u => u.Organization)
                .ToListAsync();
        }

        public async Task<List<User>> GetByOrganizationAsync(int orgNr)
        {
            return await _context.Users
                .Where(u => u.OrgNr == orgNr)
                .ToListAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}