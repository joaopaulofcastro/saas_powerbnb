using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaaS.PowerBnB.Modules.Charging.Domain.Entities;
using SaaS.PowerBnB.Modules.Charging.Infrastructure.Data;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Communication.Events;

namespace SaaS.PowerBnB.Modules.Charging.Features.UserRegistration;

internal class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly ChargingDbContext _dbContext;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(
        ChargingDbContext dbContext,
        ILogger<UserRegisteredEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        // Idempotência: verifica se o ChargingUser já existe
        var exists = await _dbContext.ChargingUsers
            .AnyAsync(u => u.Id == notification.UserId, cancellationToken);

        if (exists)
        {
            _logger.LogInformation(
                "ChargingUser {UserId} já existe. Evento ignorado (idempotência).",
                notification.UserId);
            return;
        }

        var chargingUser = new ChargingUser(notification.UserId);
        await _dbContext.ChargingUsers.AddAsync(chargingUser, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ChargingUser criado com sucesso. UserId: {UserId}",
            notification.UserId);
    }
}
