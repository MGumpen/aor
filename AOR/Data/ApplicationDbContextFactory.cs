using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AOR.Data
{
    public class AorDbContextFactory : IDesignTimeDbContextFactory<AorDbContext>
    {
        public AorDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AorDbContext>()
                .UseMySql(
                    // Dev-connection string (lokalt, ikke Docker-hostnavn)
                    "Server=localhost;Port=3306;Database=aor_db;User=aor_user;Password=Test123;CharSet=utf8mb4;",
                    new MySqlServerVersion(new Version(11, 4, 0))
                )
                .Options;

            return new AorDbContext(options);
        }
    }
}