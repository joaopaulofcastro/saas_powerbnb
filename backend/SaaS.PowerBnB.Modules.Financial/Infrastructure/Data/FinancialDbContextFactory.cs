using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SaaS.PowerBnB.Modules.Financial.Infrastructure.Data;

internal class FinancialDbContextFactory : IDesignTimeDbContextFactory<FinancialDbContext>
{
    public FinancialDbContext CreateDbContext(string[] args)
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

        var builder = new DbContextOptionsBuilder<FinancialDbContext>();
        builder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

        return new FinancialDbContext(builder.Options);
    }
}
