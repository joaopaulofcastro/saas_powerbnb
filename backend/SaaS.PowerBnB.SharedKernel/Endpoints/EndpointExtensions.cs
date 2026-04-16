using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SaaS.PowerBnB.SharedKernel.Endpoints;

public static class EndpointExtensions
{
    //Registra as classes na Injeção de Dependência
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var endpointTypes = assembly.GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in endpointTypes)
        {
            services.AddTransient(typeof(IEndpoint), type);
        }

        return services;
    }

    //Mapeia as rotas no pipeline do ASP.NET
    public static IApplicationBuilder MapEndpoints(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var endpoints = scope.ServiceProvider.GetServices<IEndpoint>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}