using AOR.Models.Data;

namespace AOR.Repositories
{
    /// <summary>
    /// Interface for dataaaksess til organisasjoner.
    /// Definerer CRUD-operasjoner for OrgModel.
    /// </summary>
    public interface IOrganizationRepository
    {
        /// <summary>
        /// Henter alle organisasjoner fra databasen.
        /// </summary>
        Task<List<OrgModel>> GetAllAsync();
        
        /// <summary>
        /// Henter en spesifikk organisasjon basert på organisasjonsnummeret.
        /// </summary>
        /// <param name="orgNr">Organisasjonsnummeret</param>
        Task<OrgModel?> GetByOrgNrAsync(int orgNr);
        
        /// <summary>
        /// Sjekker om en organisasjon eksisterer i databasen.
        /// </summary>
        /// <param name="orgNr">Organisasjonsnummeret</param>
        Task<bool> ExistsAsync(int orgNr);
        
        /// <summary>
        /// Opprett en ny organisasjon i databasen.
        /// </summary>
        Task AddAsync(OrgModel org);
        
        /// <summary>
        /// Oppdater en eksisterende organisasjon.
        /// </summary>
        Task UpdateAsync(OrgModel org);
        
        /// <summary>
        /// Sletter en organisasjon fra databasen.
        /// </summary>
        Task DeleteAsync(int orgNr);
    }
}