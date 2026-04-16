using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.PowerBnB.Modules.Charging.Domain.Entities;
using SaaS.PowerBnB.SharedKernel.Domain.Constants;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Data.Configurations;


namespace SaaS.PowerBnB.Modules.Charging.Infrastructure.Data.Configurations;

internal class ChargingPointConfiguration : AggregateRootConfiguration<ChargingPoint>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ChargingPoint> builder)
    {
        // Tabela
        builder.ToTable("charging_points");

        // Propriedades Básicas
        builder.Property(c => c.Title).IsRequired().HasMaxLength(150);
        builder.Property(c => c.MaxPowerKw).IsRequired().HasPrecision(5, 2);
        builder.Property(c => c.PricePerKwh).HasPrecision(10, 2);
        builder.Property(c => c.Location).HasColumnType($"geography(Point, {SpatialConstants.Wgs84})").IsRequired();

        builder.Property(c => c.Connector).IsRequired();
        builder.Property(c => c.Status).IsRequired();
        builder.Property(c => c.LastPingAt);

        // Índices
        builder.HasIndex(c => c.HostId);
        builder.HasIndex(c => c.Location).HasMethod("GIST");
    }
}