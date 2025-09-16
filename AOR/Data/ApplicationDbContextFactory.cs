using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AOR.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Use a temporary connection string for design-time operations
        optionsBuilder.UseMySql(
            "Server=localhost;Database=aor_db;Uid=root;Pwd=rootpassword123;Port=3306;",
            ServerVersion.AutoDetect("Server=localhost;Database=aor_db;Uid=root;Pwd=rootpassword123;Port=3306;")
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}