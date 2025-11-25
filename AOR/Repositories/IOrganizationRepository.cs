using AOR.Models;

namespace AOR.Repositories
{
    public interface IOrganizationRepository
    {
        Task<List<OrgModel>> GetAllAsync();
        Task<OrgModel?> GetByOrgNrAsync(int orgNr);
        Task<bool> ExistsAsync(int orgNr);
        Task AddAsync(OrgModel org);
        Task UpdateAsync(OrgModel org);
        Task DeleteAsync(int orgNr);
    }
}