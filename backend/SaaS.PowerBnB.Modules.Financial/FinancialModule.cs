using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaaS.PowerBnB.Modules.Financial.Infrastructure.Data;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Interceptors;

namespace SaaS.PowerBnB.Modules.Financial;

public static class FinancialModule
{
    public static IServiceCollection AddFinancialModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. DbContext isolado do módulo (schema: financial)
        services.AddDbContext<FinancialDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PowerBnbDb"));
            options.UseSnakeCaseNamingConvention();

            var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            var historyInterceptor = sp.GetRequiredService<AuditHistoryInterceptor>();
            options.AddInterceptors(auditableInterceptor, historyInterceptor);
        });

        // 2. MediatR Handlers do módulo (UserRegisteredEventHandler)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(FinancialModule).Assembly);
        });

        return services;
    }

    public static IApplicationBuilder UseFinancialModule(this IApplicationBuilder app)
    {
        return app;
    }
}
