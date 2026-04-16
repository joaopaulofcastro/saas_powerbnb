using FluentValidation;

namespace SaaS.PowerBnB.Modules.Charging.Features.RegisterPoint;

internal class RegisterPointValidator : AbstractValidator<RegisterPointCommand>
{
    public RegisterPointValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MinimumLength(5);
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.Lon).InclusiveBetween(-180, 180);
    }
}