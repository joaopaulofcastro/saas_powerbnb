using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SaaS.PowerBnB.Modules.Charging.Domain.Enums;
using SaaS.PowerBnB.SharedKernel.Application.Interfaces;
using SaaS.PowerBnB.SharedKernel.Endpoints;

namespace SaaS.PowerBnB.Modules.Charging.Features.UpdatePoint;

internal class UpdatePointEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/charging-points/{id:guid}", async (
            Guid id,
            UpdatePointRequest body,
            IMediator mediator,
            ICurrentUserService currentUserService) =>
        {
            var command = new UpdatePointCommand(
                Id: id,
                Title: body.Title,
                Connector: body.Connector,
                MaxPowerKw: body.MaxPowerKw,
                PricePerKwh: body.PricePerKwh,
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

// DTO de entrada para o endpoint (separado do Command para não expor HostId)
internal record UpdatePointRequest(
    string Title,
    ConnectorType Connector,
    decimal MaxPowerKw,
    decimal PricePerKwh
);
