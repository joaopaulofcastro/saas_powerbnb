using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SaaS.PowerBnB.SharedKernel.Application.Interfaces;
using SaaS.PowerBnB.SharedKernel.Endpoints;


namespace SaaS.PowerBnB.Modules.Charging.Features.RegisterPoint;

internal class RegisterPointEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/charging-points", async (RegisterPointCommand command, IMediator mediator, ICurrentUserService currentUserService) =>
        {
            var secureCommand = command with { HostId = Guid.Parse(currentUserService.UserId!) };
            var result = await mediator.Send(secureCommand);

            return result.Match(
                pointId => Results.Created($"/api/charging-points/{pointId}", new { Id = pointId }),
                validationFailed => validationFailed.ToProblemDetails()
            );
        })
        .WithTags("Charging Points")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
    }
}