using Microsoft.AspNetCore.Routing;

namespace SaaS.PowerBnB.SharedKernel.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}