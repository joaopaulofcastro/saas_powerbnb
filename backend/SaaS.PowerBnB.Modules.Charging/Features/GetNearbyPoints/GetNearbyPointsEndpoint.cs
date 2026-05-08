using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using SaaS.PowerBnB.SharedKernel.Endpoints;

namespace SaaS.PowerBnB.Modules.Charging.Features.GetNearbyPoints;

internal class GetNearbyPointsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/charging-points/nearby", async (
            double lat,
            double lon,
            double radiusKm,
            IMediator mediator,
            IConfiguration configuration,
            IValidator<GetNearbyPointsQuery> validator) =>
        {
            var query = new GetNearbyPointsQuery(lat, lon, radiusKm, configuration);

            // Valida antes de enviar ao pipeline (a query não usa OneOf, então validamos aqui)
            var validationResult = await validator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => new { code = e.PropertyName, description = e.ErrorMessage });
                return Results.BadRequest(new { errors });
            }

            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithTags("Charging Points")
        .Produces<IReadOnlyList<NearbyPointDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
    }
}
