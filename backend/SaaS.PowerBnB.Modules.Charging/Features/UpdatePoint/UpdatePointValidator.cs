using FluentValidation;

namespace SaaS.PowerBnB.Modules.Charging.Features.UpdatePoint;

internal class UpdatePointValidator : AbstractValidator<UpdatePointCommand>
{
    public UpdatePointValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título não pode ser vazio.")
            .MinimumLength(3).WithMessage("O título deve ter no mínimo 3 caracteres.")
            .MaximumLength(150).WithMessage("O título deve ter no máximo 150 caracteres.");

        RuleFor(x => x.MaxPowerKw)
            .GreaterThan(0).WithMessage("A potência máxima deve ser maior que zero.");

        RuleFor(x => x.PricePerKwh)
            .GreaterThan(0).WithMessage("O preço por kWh deve ser maior que zero.");
    }
}
