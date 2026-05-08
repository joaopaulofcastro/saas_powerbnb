using MediatR;
using OneOf;
using SaaS.PowerBnB.Modules.Identity.Domain.Entities;
using SaaS.PowerBnB.Modules.Identity.Infrastructure.Repositories;
using SaaS.PowerBnB.SharedKernel.Application.Errors;

namespace SaaS.PowerBnB.Modules.Identity.Features.RegisterUser;

internal class RegisterUserHandler
    : IRequestHandler<RegisterUserCommand, OneOf<Guid, ValidationFailed>>
{
    private readonly IUserProfileRepository _repository;

    public RegisterUserHandler(IUserProfileRepository repository)
        => _repository = repository;

    public async Task<OneOf<Guid, ValidationFailed>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // Idempotência: se o perfil já existe, retorna o Id sem criar duplicata
        var existing = await _repository.GetByIdAsync(request.UserId, cancellationToken);
        if (existing is not null)
            return existing.Id;

        // Cria o agregado — o Id é o sub do Keycloak, não gerado pelo sistema
        var userProfile = new UserProfile(request.UserId, request.Email, request.Name);
        await _repository.AddAsync(userProfile, cancellationToken);

        // SaveChangesAsync é responsabilidade do IdentityTransactionBehavior
        return userProfile.Id;
    }
}
