using AOR.Data;
using AOR.Models.Data;

namespace AOR.Repositories
{
    public interface IReportRepository
    {
        // Hent alle rapporter med tilhørende Obstacle, User og Status
        Task<List<ReportModel>> GetAllWithIncludesAsync();

        // Hent rapporter for én bestemt bruker (MyReports)
        Task<List<ReportModel>> GetByUserAsync(string userId);

        // Hent rapporter der brukeren er assignet (AssignedToId)
        Task<List<ReportModel>> GetAssignedToAsync(string userId);

        // Hent antall rapporter som er assignet til en bruker og fortsatt er Pending
        Task<int> GetAssignedPendingCountAsync(string userId);

        // Hent antall rapporter som er Pending og ikke er assignet til noen
        Task<int> GetUnassignedPendingCountAsync();

        // Hent alle brukere som har rollen Registrar
        Task<List<User>> GetRegistrarsAsync();

        // Assign en rapport til en registrar (satt AssignedToId)
        Task AssignToAsync(int reportId, string? registrarUserId);

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