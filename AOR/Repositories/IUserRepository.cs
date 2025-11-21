using AOR.Data;

namespace AOR.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllWithOrganizationAsync();
        Task<List<User>> GetByOrganizationAsync(int orgNr);
        Task UpdateAsync(User user);
    }
}