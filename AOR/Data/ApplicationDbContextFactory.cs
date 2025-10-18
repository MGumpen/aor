using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AOR.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // 1) Miljøvariabel (Docker/CLI override)
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        // 2) appsettings.{ENV}.json / appsettings.json
        if (string.IsNullOrWhiteSpace(cs))
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            cs = config.GetConnectionString("DefaultConnection");
        }

        // 3) Fallback: lokal Docker-port (host) med aor_user
        if (string.IsNullOrWhiteSpace(cs))
        {
            cs = "Server=127.0.0.1;Port=3307;Database=aor_db;User=aor_user;Password=Test123;SslMode=None";
        }

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(cs, ServerVersion.AutoDetect(cs));

        return new ApplicationDbContext(builder.Options);
    }
}