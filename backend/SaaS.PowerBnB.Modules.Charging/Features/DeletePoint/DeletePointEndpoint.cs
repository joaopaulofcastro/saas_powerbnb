using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SaaS.PowerBnB.SharedKernel.Application.Interfaces;
using SaaS.PowerBnB.SharedKernel.Endpoints;

namespace SaaS.PowerBnB.Modules.Charging.Features.DeletePoint;

internal class DeletePointEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/charging-points/{id:guid}", async (
            Guid id,
            IMediator mediator,
            ICurrentUserService currentUserService) =>
        {
            var command = new DeletePointCommand(
                Id: id,
                HostId: Guid.Parse(currentUserService.UserId!));

            var result = await mediator.Send(command);

            return result.Match(
                _ => Results.NoContent(),
                validationFailed => validationFailed.ToProblemDetails()
            );
        })
        .WithTags("Charging Points")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
    }
}
