using AOR.Data;
using AOR.Models.Data;

namespace AOR.Repositories
{
    /// <summary>
    /// Interface for dataaaksess til rapporter.
    /// Definer alle operasjoner for CRUD og spesialiserte queries på ReportModel.
    /// </summary>
    public interface IReportRepository
    {
        /// <summary>
        /// Henter alle rapporter med tilhørende Obstacle, User og Status (eager loading).
        /// </summary>
        Task<List<ReportModel>> GetAllWithIncludesAsync();

        /// <summary>
        /// Henter rapporter som en bestemt bruker har opprettet (MyReports).
        /// </summary>
        /// <param name="userId">ID-en til brukeren</param>
        Task<List<ReportModel>> GetByUserAsync(string userId);

        /// <summary>
        /// Henter rapporter som er assignet til en bestemt registrar.
        /// </summary>
        /// <param name="userId">ID-en til registraren</param>
        Task<List<ReportModel>> GetAssignedToAsync(string userId);

        /// <summary>
        /// Henter antall rapporter assignet til en bruker med status Pending.
        /// </summary>
        /// <param name="userId">ID-en til registraren</param>
        Task<int> GetAssignedPendingCountAsync(string userId);

        /// <summary>
        /// Henter antall rapporter som ikke er assignet til noen og har status Pending.
        /// </summary>
        Task<int> GetUnassignedPendingCountAsync();

        /// <summary>
        /// Henter alle brukere som har rollen Registrar.
        /// </summary>
        Task<List<UserModel>> GetRegistrarsAsync();

        /// <summary>
        /// Assignerer en rapport til en registrar eller fjerner assigneringen.
        /// </summary>
        /// <param name="reportId">Rapporten som skal assigneres</param>
        /// <param name="registrarUserId">ID-en til registraren (null for å fjerne assignering)</param>
        Task AssignToAsync(int reportId, string? registrarUserId);

        /// <summary>
        /// Henter rapporter som ble opprettet de siste 30 dagene.
        /// </summary>
        Task<List<ReportModel>> GetLast30DaysAsync();

        /// <summary>
        /// Henter en spesifikk rapport med alle relaterte data (for detaljer-visning).
        /// </summary>
        /// <param name="reportId">Rapporten som skal hentes</param>
        Task<ReportModel?> GetByIdWithIncludesAsync(int reportId);

        /// <summary>
        /// Opprett en ny rapport i databasen.
        /// </summary>
        Task AddAsync(ReportModel report);

        /// <summary>
        /// Oppdater en eksisterende rapport.
        /// </summary>
        Task UpdateAsync(ReportModel report);

        /// <summary>
        /// Oppdater kun statusen på en rapport.
        /// </summary>
        /// <param name="reportId">Rapporten som skal oppdateres</param>
        /// <param name="statusId">Ny status-ID</param>
        Task UpdateStatusAsync(int reportId, int statusId);

        /// <summary>
        /// Sletter en rapport fra databasen.
        /// </summary>
        Task DeleteAsync(int reportId);
    }
}