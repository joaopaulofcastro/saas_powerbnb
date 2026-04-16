using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaaS.PowerBnB.Modules.Charging.Behaviors;
using SaaS.PowerBnB.Modules.Charging.Infrastructure.Data;
using SaaS.PowerBnB.Modules.Charging.Infrastructure.Workers;
using SaaS.PowerBnB.SharedKernel.Data;
using SaaS.PowerBnB.SharedKernel.Endpoints;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Interceptors;

namespace SaaS.PowerBnB.Modules.Charging;

public static class ChargingModule
{
    public static IServiceCollection AddChargingModule(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Injeção de Dependência Automática com SCRUTOR
        services.Scan(scan => scan
            // Diz ao Scrutor para olhar apenas para os arquivos dentro da pasta deste módulo
            .FromAssemblies(typeof(ChargingModule).Assembly)

            // Regra A: Acha tudo que termina com "Repository" e registra como Scoped
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")), publicOnly: false)
                .AsMatchingInterface()
                .AsImplementedInterfaces()
                .WithScopedLifetime()

            // Regra B: Acha tudo que termina com "Service" (ex: Domain Services) e registra como Scoped
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")), publicOnly: false)
                .AsMatchingInterface()
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        // 2. Banco de Dados isolado do Módulo
        services.AddDbContext<ChargingDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PowerBnbDb"),
                postgresOptions =>
                {
                    postgresOptions.UseNetTopologySuite();
                });

            //Para criação das tabelas AuditLogs (audit_logs) e OutboxMessage (outbox_message)
            options.UseSnakeCaseNamingConvention();

            var outboxInterceptor = sp.GetRequiredService<InsertOutboxMessagesInterceptor>();
            var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            var historyInterceptor = sp.GetRequiredService<AuditHistoryInterceptor>();

            options.AddInterceptors(outboxInterceptor, auditableInterceptor, historyInterceptor);
        });

        // 3. UnitOfWork acoplado ao banco do módulo
        services.AddScoped<IUnitOfWork<ChargingDbContext>, UnitOfWork<ChargingDbContext>>();

        // 4. O Módulo registra seus PRÓPRIOS Handlers no MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ChargingModule).Assembly);
        });

        // 5. O BEHAVIOR DE TRANSAÇÃO ESPECÍFICO DO MÓDULO 
        // Ele só vai interceptar classes que herdem de IChargingCommand.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ChargingTransactionBehavior<,>));

        // 6. Worker do Outbox específico deste módulo
        services.AddHostedService<ChargingOutboxWorker>();

        // 7. Auto-Discovery de Endpoints (A nossa extensão do Shared Kernel)
        services.AddEndpoints(typeof(ChargingModule).Assembly);

        return services;
    }

    public static IApplicationBuilder UseChargingModule(this IApplicationBuilder app)
    {
        return app;
    }
}
