using MediatR;
using OneOf;
using SaaS.PowerBnB.Modules.Identity.CQRS;
using SaaS.PowerBnB.SharedKernel.Application.Errors;

namespace SaaS.PowerBnB.Modules.Identity.Features.RegisterUser;

internal record RegisterUserCommand(
    Guid UserId,
    string Email,
    string Name
) : IIdentityCommand<OneOf<Guid, ValidationFailed>>;
