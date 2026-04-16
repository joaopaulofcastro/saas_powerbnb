using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace SaaS.PowerBnB.Modules.Charging.Infrastructure.Data;

internal class ChargingDbContextFactory : IDesignTimeDbContextFactory<ChargingDbContext>
{
    public ChargingDbContext CreateDbContext(string[] args)
    {
        // 1. Descobre em qual ambiente estamos rodando (geralmente Development na máquina local)
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        // 2. Monta o leitor de configurações apontando para a pasta de onde o comando foi rodado
        // Como combinamos de rodar o 'dotnet ef' de dentro da pasta SaaS.PowerBnB.Api, 
        // ele vai achar os arquivos perfeitamente.
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        // 3. Lê a connection string exatamente com o mesmo nome que você colocou no appsettings
        var connectionString = configuration.GetConnectionString("PowerBnbDb");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("A ConnectionString 'PowerBnbDb' não foi encontrada no appsettings.");
        }

        // 4. Constrói o DbContext
        var builder = new DbContextOptionsBuilder<ChargingDbContext>();

        builder.UseNpgsql(connectionString, postgresOptions =>
            postgresOptions.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention();

        return new ChargingDbContext(builder.Options);
    }
}