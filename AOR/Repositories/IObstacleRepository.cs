using AOR.Models.Data;

namespace AOR.Repositories
{
    /// <summary>
    /// Interface for dataaaksess til hindringer.
    /// Definerer CRUD-operasjoner for ObstacleDataModel.
    /// </summary>
    public interface IObstacleRepository
    {
        /// <summary>
        /// Henter alle hindringer fra databasen.
        /// </summary>
        Task<List<ObstacleDataModel>> GetAllAsync();

        /// <summary>
        /// Henter en spesifikk hindring basert på ID.
        /// </summary>
        /// <param name="obstacleId">Hindringens ID</param>
        Task<ObstacleDataModel?> GetByIdAsync(int obstacleId);

        /// <summary>
        /// Sjekker om en hindring eksisterer i databasen.
        /// </summary>
        /// <param name="obstacleId">Hindringens ID</param>
        Task<bool> ExistsAsync(int obstacleId);

        /// <summary>
        /// Opprett en ny hindring i databasen.
        /// </summary>
        Task AddAsync(ObstacleDataModel obstacle);

        /// <summary>
        /// Oppdater en eksisterende hindring.
        /// </summary>
        Task UpdateAsync(ObstacleDataModel obstacle);

        /// <summary>
        /// Sletter en hindring fra databasen.
        /// </summary>
        Task DeleteAsync(int obstacleId);
    }
}