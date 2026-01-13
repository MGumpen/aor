using AOR.Data;
using AOR.Models.Data;

namespace AOR.Repositories
{
    /// <summary>
    /// Interface for dataaaksess til brukere.
    /// Inneholder operasjoner for å hente og oppdatere brukerdata.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Henter alle brukere med tilhørende organisasjon (eager loading).
        /// </summary>
        Task<List<UserModel>> GetAllWithOrganizationAsync();
        
        /// <summary>
        /// Henter alle brukere som tilhører en bestemt organisasjon.
        /// </summary>
        /// <param name="orgNr">Organisasjonsnummeret</param>
        Task<List<UserModel>> GetByOrganizationAsync(int orgNr);
        
        /// <summary>
        /// Oppdaterer en eksisterende bruker i databasen.
        /// </summary>
        Task<UserModel> UpdateAsync(UserModel user);
    }
}