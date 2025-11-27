using AOR.Models.Data;

namespace AOR.Repositories
{
    public interface IReportRepository
    {
        // Hent alle rapporter med tilhørende Obstacle, User og Status
        Task<List<ReportModel>> GetAllWithIncludesAsync();

        // Hent rapporter for én bestemt bruker (MyReports)
        Task<List<ReportModel>> GetByUserAsync(string userId);

        // Hent rapporter siste 30 dager (for Last30Days-endepunktet)
        Task<List<ReportModel>> GetLast30DaysAsync();

        // Hent én spesifikk rapport inkl. Obstacle, User, Status (til detaljer-visning)
        Task<ReportModel?> GetByIdWithIncludesAsync(int reportId);

        // Opprett ny rapport
        Task AddAsync(ReportModel report);

        // Oppdater en eksisterende rapport (f.eks. endre felter)
        Task UpdateAsync(ReportModel report);

        // Bare oppdatere status på en rapport (nyttig for admin/moderering)
        Task UpdateStatusAsync(int reportId, int statusId);

        // Slette rapport (om dere ønsker det senere)
        Task DeleteAsync(int reportId);
    }
}