using AOR.Models.Data;

namespace AOR.Repositories
{
    public interface IObstacleRepository
    {
        // Hente alle hinder
        Task<List<ObstacleData>> GetAllAsync();

        // Hente ett hinder
        Task<ObstacleData?> GetByIdAsync(int obstacleId);

        // Sjekke om et hinder finnes
        Task<bool> ExistsAsync(int obstacleId);

        // Opprette nytt hinder
        Task AddAsync(ObstacleData obstacle);

        // Oppdatere eksisterende hinder
        Task UpdateAsync(ObstacleData obstacle);

        // Slette hinder
        Task DeleteAsync(int obstacleId);
    }
}