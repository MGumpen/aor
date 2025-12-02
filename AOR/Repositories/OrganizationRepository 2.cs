using AOR.Data;
using AOR.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace AOR.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly AorDbContext _context;

        public OrganizationRepository(AorDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrgModel>> GetAllAsync()
        {
            return await _context.Organizations
                .OrderBy(o => o.OrgNr)
                .ToListAsync();
        }

        public async Task<OrgModel?> GetByOrgNrAsync(int orgNr)
        {
            return await _context.Organizations.FindAsync(orgNr);
        }

        public async Task<bool> ExistsAsync(int orgNr)
        {
            return await _context.Organizations.AnyAsync(o => o.OrgNr == orgNr);
        }

        public async Task AddAsync(OrgModel org)
        {
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrgModel org)
        {
            _context.Organizations.Update(org);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int orgNr)
        {
            var org = await _context.Organizations.FindAsync(orgNr);
            if (org == null) return;

            _context.Organizations.Remove(org);
            await _context.SaveChangesAsync();
        }
    }
}