using AOR.Data;
using AOR.Models;
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
                .Include(r => r.User)
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

        // Hente én spesifikk rapport med relasjoner (til detaljer-view)
        public async Task<ReportModel?> GetByIdWithIncludesAsync(int reportId)
        {
            return await _context.Reports
                .AsNoTracking()
                .Include(r => r.Obstacle)
                .Include(r => r.User)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);
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
            _context.Reports.Update(report);
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