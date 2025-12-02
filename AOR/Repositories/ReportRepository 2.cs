using AOR.Data;
using AOR.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace AOR.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AorDbContext _context;

        public ReportRepository(AorDbContext context)
        {
            _context = context;
        }

        // Hente alle rapporter med relasjoner (til f.eks. admin/dashbord)
        public async Task<List<ReportModel>> GetAllWithIncludesAsync()
        {
            return await _context.Reports
                .AsNoTracking()
                .Include(r => r.Obstacle)
                .Include(r => r.User).ThenInclude(u => u.Organization)
                .Include(r => r.AssignedTo).ThenInclude(u => u.Organization)
                .Include(r => r.Status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // Hente alle rapporter for én bestemt bruker (MyReports)
        public async Task<List<ReportModel>> GetByUserAsync(string userId)
        {
            return await _context.Reports
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .Include(r => r.Obstacle)
                .Include(r => r.Status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // Hente rapporter siste 30 dager (inkl. obstacle)
        public async Task<List<ReportModel>> GetLast30DaysAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);

            return await _context.Reports
                .AsNoTracking()
                .Include(r => r.Obstacle)
                .Where(r => r.CreatedAt >= cutoffDate)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // Hent rapporter der en bruker er assignet (AssignedToId)
        public async Task<List<ReportModel>> GetAssignedToAsync(string userId)
        {
            return await _context.Reports
                .AsNoTracking()
                .Where(r => r.AssignedToId == userId)
                .Include(r => r.Obstacle)
                .Include(r => r.User).ThenInclude(u => u.Organization)
                .Include(r => r.AssignedTo).ThenInclude(u => u.Organization)
                .Include(r => r.Status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // Hent antall rapporter som er assignet til en bruker og fortsatt er Pending
        public async Task<int> GetAssignedPendingCountAsync(string userId)
        {
            // Assuming Pending has StatusId = 1 (seeded in DbContext)
            return await _context.Reports
                .AsNoTracking()
                .Where(r => r.AssignedToId == userId && r.StatusId == 1)
                .CountAsync();
        }

        // Hent antall rapporter som er Pending og ikke er assignet til noen
        public async Task<int> GetUnassignedPendingCountAsync()
        {
            return await _context.Reports
                .AsNoTracking()
                .Where(r => r.AssignedToId == null && r.StatusId == 1)
                .CountAsync();
        }

        // Hent alle brukere som har rollen Registrar
        public async Task<List<User>> GetRegistrarsAsync()
        {
            // Tabellen for AspNetUserRoles er tilgjengelig som _context.UserRoles
            var registrarRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Registrar");
            if (registrarRole == null) return new List<User>();

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == registrarRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Include(u => u.Organization)
                .ToListAsync();
        }

        // Assign en rapport til en registrar (setter AssignedToId)
        public async Task AssignToAsync(int reportId, string? registrarUserId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return;

            // Hvis registrarUserId er tom eller null -> fjern tildeling
            report.AssignedToId = string.IsNullOrWhiteSpace(registrarUserId) ? null : registrarUserId;
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }

        // Hente én spesifikk rapport med relasjoner (til detaljer-view)
        public async Task<ReportModel?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Reports
                .Include(r => r.Obstacle)
                .Include(r => r.User)
                .ThenInclude(u => u.Organization)
                .Include(r => r.AssignedTo).ThenInclude(u => u.Organization)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.ReportId == id);
        }

        // Opprette ny rapport
        public async Task AddAsync(ReportModel report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
        }

        // Oppdatere en rapport
        public async Task UpdateAsync(ReportModel report)
        {
            _context.Entry(report).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Kun endre status på en rapport
        public async Task UpdateStatusAsync(int reportId, int statusId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                return;
            }

            report.StatusId = statusId;
            await _context.SaveChangesAsync();
        }

        // Slette rapport
        public async Task DeleteAsync(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                return;
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
        }
    }
}