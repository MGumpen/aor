using AOR.Data;
using AOR.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace AOR.Repositories
{
    /// <summary>
    /// Implementerer dataaaksess-logikk for brukere.
    /// Håndterer henting og oppdatering av brukerdata fra databasen.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AorDbContext _context;

        /// <summary>
        /// Initialiserer repository med DbContext.
        /// </summary>
        public UserRepository(AorDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Henter alle brukere med deres tilhørende organisasjoner (eager loading).
        /// </summary>
        public async Task<List<UserModel>> GetAllWithOrganizationAsync()
        {
            return await _context.Users
                .Include(u => u.Organization)
                .ToListAsync();
        }

        /// <summary>
        /// Henter alle brukere som tilhører en spesifikk organisasjon.
        /// </summary>
        /// <param name="orgNr">Organisasjonsnummeret</param>
        public async Task<List<UserModel>> GetByOrganizationAsync(int orgNr)
        {
            return await _context.Users
                .Where(u => u.OrgNr == orgNr)
                .ToListAsync();
        }

        /// <summary>
        /// Oppdaterer en bruker i databasen og returnerer den oppdaterte brukeren.
        /// </summary>
        public async Task<UserModel> UpdateAsync(UserModel user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}