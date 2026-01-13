using AOR.Data;
using AOR.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace AOR.Repositories
{
    /// <summary>
    /// Implementerer dataaaksess-logikk for rapporter.
    /// Håndterer alle CRUD-operasjoner og spesialiserte queries.
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly AorDbContext _context;

        /// <summary>
        /// Initialiserer repository med DbContext.
        /// </summary>
        public ReportRepository(AorDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Henter alle rapporter med relaterte objekter (for admin-dashbord).
        /// </summary>
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

        /// <summary>
        /// Henter alle rapporter opprettet av en spesifikk bruker.
        /// </summary>
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

        /// <summary>
        /// Henter rapporter opprettet de siste 30 dagene.
        /// </summary>
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

        /// <summary>
        /// Henter rapporter som er assignet til en spesifikk registrar.
        /// </summary>
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

        /// <summary>
        /// Teller antall rapporter assignet til en bruker med status Pending.
        /// </summary>
        public async Task<int> GetAssignedPendingCountAsync(string userId)
        {
            // StatusId 1 = Pending
            return await _context.Reports
                .AsNoTracking()
                .Where(r => r.AssignedToId == userId && r.StatusId == 1)
                .CountAsync();
        }

        /// <summary>
        /// Teller antall rapporter som ikke er assignet og har status Pending.
        /// </summary>
        public async Task<int> GetUnassignedPendingCountAsync()
        {
            return await _context.Reports
                .AsNoTracking()
                .Where(r => r.AssignedToId == null && r.StatusId == 1)
                .CountAsync();
        }

        /// <summary>
        /// Henter alle brukere som har rollen Registrar.
        /// </summary>
        public async Task<List<UserModel>> GetRegistrarsAsync()
        {
            var registrarRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Registrar");
            if (registrarRole == null) return new List<UserModel>();

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == registrarRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Include(u => u.Organization)
                .ToListAsync();
        }

        /// <summary>
        /// Tildeler en rapport til en registrar eller fjerner tildelingen.
        /// </summary>
        public async Task AssignToAsync(int reportId, string? registrarUserId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return;

            report.AssignedToId = string.IsNullOrWhiteSpace(registrarUserId) ? null : registrarUserId;
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Henter en enkelt rapport med alle relaterte objekter.
        /// </summary>
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

        /// <summary>
        /// Opprett en ny rapport i databasen.
        /// </summary>
        public async Task AddAsync(ReportModel report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Oppdater en eksisterende rapport.
        /// </summary>
        public async Task UpdateAsync(ReportModel report)
        {
            _context.Entry(report).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Oppdater kun statusen på en rapport.
        /// </summary>
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

        /// <summary>
        /// Sletter en rapport fra databasen.
        /// </summary>
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