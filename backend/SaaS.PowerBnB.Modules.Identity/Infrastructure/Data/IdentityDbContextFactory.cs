using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SaaS.PowerBnB.Modules.Identity.Infrastructure.Data;

internal class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("PowerBnbDb");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("A ConnectionString 'PowerBnbDb' não foi encontrada.");

        var builder = new DbContextOptionsBuilder<IdentityDbContext>();
        builder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

        return new IdentityDbContext(builder.Options);
    }
}
