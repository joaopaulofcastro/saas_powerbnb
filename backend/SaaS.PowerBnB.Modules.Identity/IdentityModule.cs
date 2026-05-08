using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaaS.PowerBnB.Modules.Identity.Behaviors;
using SaaS.PowerBnB.Modules.Identity.Infrastructure.Data;
using SaaS.PowerBnB.Modules.Identity.Infrastructure.Workers;
using SaaS.PowerBnB.SharedKernel.Data;
using SaaS.PowerBnB.SharedKernel.Endpoints;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Interceptors;

namespace SaaS.PowerBnB.Modules.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Auto-discovery de Repositories e Services via Scrutor
        services.Scan(scan => scan
            .FromAssemblies(typeof(IdentityModule).Assembly)
            .AddClasses(c => c.Where(t => t.Name.EndsWith("Repository")), publicOnly: false)
                .AsMatchingInterface()
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.Where(t => t.Name.EndsWith("Service")), publicOnly: false)
                .AsMatchingInterface()
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        // 2. DbContext isolado do módulo (schema: identity)
        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PowerBnbDb"));
            options.UseSnakeCaseNamingConvention();

            var outboxInterceptor = sp.GetRequiredService<InsertOutboxMessagesInterceptor>();
            var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            var historyInterceptor = sp.GetRequiredService<AuditHistoryInterceptor>();

            options.AddInterceptors(outboxInterceptor, auditableInterceptor, historyInterceptor);
        });

        // 3. UnitOfWork acoplado ao banco do módulo
        services.AddScoped<IUnitOfWork<IdentityDbContext>, UnitOfWork<IdentityDbContext>>();

        // 4. MediatR Handlers do módulo
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(IdentityModule).Assembly);
        });

        // 4.1 Validators do FluentValidation
        services.AddValidatorsFromAssembly(typeof(IdentityModule).Assembly, includeInternalTypes: true);

        // 5. Transaction Behavior específico do módulo (intercepta apenas IIdentityCommand)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdentityTransactionBehavior<,>));

        // 6. Outbox Worker
        services.AddHostedService<IdentityOutboxWorker>();

        // 7. Auto-discovery de Endpoints
        services.AddEndpoints(typeof(IdentityModule).Assembly);

        return services;
    }

    public static IApplicationBuilder UseIdentityModule(this IApplicationBuilder app)
    {
        return app;
    }
}
