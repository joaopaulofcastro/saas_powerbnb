using FluentValidation;

namespace SaaS.PowerBnB.Modules.Charging.Features.GetNearbyPoints;

internal class GetNearbyPointsValidator : AbstractValidator<GetNearbyPointsQuery>
{
    public GetNearbyPointsValidator()
    {
        RuleFor(x => x.Lat)
            .InclusiveBetween(-90, 90)
            .WithMessage("A latitude deve estar entre -90 e 90.");

        RuleFor(x => x.Lon)
            .InclusiveBetween(-180, 180)
            .WithMessage("A longitude deve estar entre -180 e 180.");

        RuleFor(x => x.RadiusKm)
            .GreaterThan(0).WithMessage("O raio de busca deve ser maior que zero.")
            .LessThanOrEqualTo(50).WithMessage("O raio de busca não pode exceder 50 km.");
    }
}
